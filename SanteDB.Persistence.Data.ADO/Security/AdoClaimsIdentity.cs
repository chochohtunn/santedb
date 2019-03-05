﻿/*
 * Copyright 2015-2019 Mohawk College of Applied Arts and Technology
 *
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: JustinFyfe
 * Date: 2019-1-22
 */
using SanteDB.Core;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Attribute;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using SanteDB.OrmLite;
using SanteDB.Persistence.Data.ADO.Configuration;
using SanteDB.Persistence.Data.ADO.Data.Model.Error;
using SanteDB.Persistence.Data.ADO.Data.Model.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Authentication;

using System.Security.Principal;

namespace SanteDB.Persistence.Data.ADO.Security
{
    /// <summary>
    /// Represents a user prinicpal based on a SecurityUser domain model 
    /// </summary>
    public class AdoClaimsIdentity : SanteDBClaimsIdentity, IIdentity, ISession, IClaimsIdentity
    {
        // Trace source
        private static TraceSource s_traceSource = new TraceSource(AdoDataConstants.IdentityTraceSourceName);

        // Lock object
        private static Object s_lockObject = new object();
        
        // The security user
        private DbSecurityUser m_securityUser;
        // The authentication type
        private String m_authenticationType;
        private ISession m_session;
        // Issued on
        private DateTimeOffset m_issuedOn = DateTimeOffset.Now;
        // Expiration time
        private DateTimeOffset? m_expires = null;
        // Roles
        private List<DbSecurityRole> m_roles = null;

        // Configuration
        private static AdoPersistenceConfigurationSection s_configuration = ApplicationServiceContext.Current.GetService<IConfigurationManager>().GetSection<AdoPersistenceConfigurationSection>();

        /// <summary>
        /// Gets the internal session id
        /// </summary>
        internal byte[] SessionToken { get { return this.m_session?.Id; } }
        
        /// <summary>
        /// Gets the identifier of the session
        /// </summary>
        byte[] ISession.Id { get { return this.SessionToken; } }

        /// <summary>
        /// Gets the 
        /// </summary>
        public byte[] RefreshToken { get { return this.m_session?.RefreshToken; } }

        /// <summary>
        /// Gets the time of issuance
        /// </summary>
        public DateTimeOffset NotBefore { get { return this.m_issuedOn; } }

        /// <summary>
        /// Gets the time of issuance
        /// </summary>
        public DateTimeOffset NotAfter { get { return this.m_expires ?? this.m_issuedOn.AddMinutes(30); } }

        /// <summary>
        /// Creates a principal based on username and password
        /// </summary>
        internal static AdoClaimsIdentity Create(String userName, String password)
        {
            try
            {
                if (userName == AuthenticationContext.AnonymousPrincipal.Identity.Name ||
                    userName == AuthenticationContext.SystemPrincipal.Identity.Name)
                {
                    throw new PolicyViolationException(new GenericPrincipal(new GenericIdentity(userName), new String[0]), PermissionPolicyIdentifiers.Login, PolicyGrantType.Deny);
                }

                Guid? userId = Guid.Empty;

                using (var dataContext = s_configuration.Provider.GetWriteConnection())
                {
                    dataContext.Open();

                    // Attempt to get a user
                    var hashingService = ApplicationServiceContext.Current.GetService<IPasswordHashingService>();

                    var passwordHash = hashingService.ComputeHash(password);
                    var fnResult = dataContext.FirstOrDefault<CompositeResult<DbSecurityUser, FunctionErrorCode>>("auth_usr", userName, passwordHash, 5);

	                var user = fnResult.Object1;

					if (!String.IsNullOrEmpty(fnResult.Object2.ErrorCode))
	                {
		                if (fnResult.Object2.ErrorCode.Contains("AUTH_LCK:"))
		                {
							UpdateCache(user, dataContext);
						}

						throw new AuthenticationException(fnResult.Object2.ErrorCode);
					}

                    var roles = dataContext.Query<DbSecurityRole>(dataContext.CreateSqlStatement< DbSecurityRole>().SelectFrom()
                        .InnerJoin<DbSecurityUserRole>(o => o.Key, o => o.RoleKey)
                        .Where<DbSecurityUserRole>(o => o.UserKey == user.Key));

                    var userIdentity = new AdoClaimsIdentity(user, roles, true) { m_authenticationType = "Password" };

                    // Is user allowed to login?
                    if (user.UserClass == UserClassKeys.HumanUser)
                        new PolicyPermission(System.Security.Permissions.PermissionState.Unrestricted, PermissionPolicyIdentifiers.Login, new GenericPrincipal(userIdentity, null)).Demand();
                    else if (user.UserClass == UserClassKeys.ApplicationUser)
                        new PolicyPermission(System.Security.Permissions.PermissionState.Unrestricted, PermissionPolicyIdentifiers.LoginAsService, new GenericPrincipal(userIdentity, null)).Demand();

					// add the security user to the cache before exiting
					UpdateCache(user, dataContext);

					return userIdentity;
                }
            }
            catch (AuthenticationException e)
            {
                // TODO: Audit this
                if (e.Message.Contains("AUTH_INV:") || e.Message.Contains("AUTH_LCK:") || e.Message.Contains("AUTH_TFA:"))
                    throw new AuthenticationException(e.Message.Substring(0, e.Message.IndexOf(":")), e);
                else
                    throw;
            }
            catch (SecurityException)
            {
                // TODO: Audit this
                throw;
            }
            catch (DbException e)
            {
                s_traceSource.TraceEvent(TraceEventType.Error, e.HResult, "Database Error Creating Identity: {0}", e);
                throw new AuthenticationException(e.Message, e);
            }
            catch (Exception e)
            {
                s_traceSource.TraceEvent(TraceEventType.Error, e.HResult, e.ToString());
                throw new Exception("Creating identity failed", e);
            }
        }
        
        /// <summary>
        /// Create a claims identity from a data context user
        /// </summary>
        internal static AdoClaimsIdentity Create(DbSecurityUser user, bool isAuthenticated = false, String authenticationMethod = null, ISession session = null)
        {

            var roles = user.Context.Query<DbSecurityRole>(user.Context.CreateSqlStatement<DbSecurityRole>().SelectFrom()
                        .InnerJoin<DbSecurityUserRole>(o => o.Key, o => o.RoleKey)
                        .Where<DbSecurityUserRole>(o => o.UserKey == user.Key));
            
            return new AdoClaimsIdentity(user, roles, isAuthenticated)
            {
                m_authenticationType = authenticationMethod,
                m_issuedOn = session?.NotBefore ?? DateTimeOffset.Now,
                m_expires = session?.NotAfter ?? DateTimeOffset.Now.AddMinutes(10),
                m_session = session
            };


        }

        /// <summary>
        /// Creates an identity from a hash
        /// </summary>
        internal static AdoClaimsIdentity Create(String userName)
        {
            try
            {
                using (var dataContext = s_configuration.Provider.GetWriteConnection())
                {
                    dataContext.Open();
                    var user = dataContext.SingleOrDefault<DbSecurityUser>(u => u.ObsoletionTime == null && u.UserName.ToLower() == userName.ToLower());
                    if (user == null)
                        return null;

                    var roles = dataContext.Query<DbSecurityRole>(dataContext.CreateSqlStatement<DbSecurityRole>().SelectFrom()
                        .InnerJoin<DbSecurityUserRole>(o => o.Key, o => o.RoleKey)
                        .Where<DbSecurityUserRole>(o => o.UserKey == user.Key));

                    return new AdoClaimsIdentity(user, roles, false);
                }
            }
            catch (Exception e)
            {
                s_traceSource.TraceEvent(TraceEventType.Error, e.HResult, e.ToString());
                throw new Exception("Creating unauthorized identity failed", e);
            }
        }

        /// <summary>
        /// Private ctor
        /// </summary>
        private AdoClaimsIdentity(DbSecurityUser user, IEnumerable<DbSecurityRole> roles, bool isAuthenticated)
            : base(user.UserName, isAuthenticated, "LOCAL")
        {
            this.m_securityUser = user;
            this.m_roles = roles.ToList();
        }
        
        /// <summary>
        /// Gets the original user upon which this principal is based
        /// </summary>
        public DbSecurityUser User
        {
            get
            {
                return this.m_securityUser;
            }
        }

        /// <summary>
        /// Create an authorization context
        /// </summary>
        public IClaimsPrincipal CreateClaimsPrincipal(IEnumerable<IClaimsIdentity> otherIdentities = null)
        {

            if (!this.IsAuthenticated)
                throw new SecurityException("Principal is not authenticated");

            try
            {

                // System claims
                List<IClaim> claims = new List<IClaim>(
                    this.m_roles.Select(r => new SanteDBClaim(SanteDBClaimTypes.DefaultRoleClaimType, r.Name))
                )
                {
                    new SanteDBClaim(SanteDBClaimTypes.AuthenticationInstant, this.NotBefore.ToString("o")), // TODO: Fix this
                    new SanteDBClaim(SanteDBClaimTypes.AuthenticationMethod, this.m_authenticationType ?? "LOCAL"),
                    new SanteDBClaim(SanteDBClaimTypes.Expiration, this.NotAfter.ToString("o")), // TODO: Move this to configuration
                    new SanteDBClaim(SanteDBClaimTypes.Name, this.m_securityUser.UserName),
                    new SanteDBClaim(SanteDBClaimTypes.Sid, this.m_securityUser.Key.ToString()),
                    new SanteDBClaim(SanteDBClaimTypes.NameIdentifier, this.m_securityUser.Key.ToString()),
                    new SanteDBClaim(SanteDBClaimTypes.Actor, this.m_securityUser.UserClass.ToString()),
                };

                if (this.m_securityUser.Email != null)
                    claims.Add(new SanteDBClaim(SanteDBClaimTypes.Email, this.m_securityUser.Email));
                if (this.m_securityUser.PhoneNumber != null)
                    claims.Add(new SanteDBClaim(SanteDBClaimTypes.Telephone, this.m_securityUser.PhoneNumber));

                if (this.SessionToken != null) {
                    claims.Add(new SanteDBClaim(SanteDBClaimTypes.IsPersistent, "true"));
                    claims.Add(new SanteDBClaim(SanteDBClaimTypes.SanteDBSessionIdClaim, (this.m_session as AdoSecuritySession)?.Key.ToString() ?? BitConverter.ToString(this.SessionToken).Replace("-","")));
                }

                this.AddClaims(claims);

                var identities = new IClaimsIdentity[] { this };
                if (otherIdentities != null)
                    identities = identities.Union(otherIdentities).ToArray();

                // TODO: Demographic data for the user
                var retVal = new SanteDBClaimsPrincipal(
                        identities
                    );
                s_traceSource.TraceInformation("Created security principal from identity {0} > {1}", this, AdoClaimsIdentity.PrincipalToString(retVal));
                return retVal;
            }
            catch (Exception e)
            {
                s_traceSource.TraceEvent(TraceEventType.Error, e.HResult, e.ToString());
                throw new Exception("Creating principal from identity failed", e);
            }
        }
        
        /// <summary>
        /// Represent principal as a string
        /// </summary>
        private static String PrincipalToString(IClaimsPrincipal retVal)
        {
            using (StringWriter sw = new StringWriter())
            {
                sw.Write("{{ Identity = {0}, Claims = [", retVal.Identity);
                foreach (var itm in retVal.Claims)
                {
                    sw.Write("{{ Type = {0}, Value = {1} }}", itm.Type, itm.Value);
                    if (itm != retVal.Claims.Last()) sw.Write(",");
                }
                sw.Write("] }");
                return sw.ToString();
            }
        }

		/// <summary>
		/// Updates the cache.
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="context">The context.</param>
		private static void UpdateCache(DbSecurityUser user, DataContext context)
	    {
		    var securityUser = new SanteDB.Persistence.Data.ADO.Services.Persistence.SecurityUserPersistenceService().ToModelInstance(user, context);
		    ApplicationServiceContext.Current.GetService<IDataCachingService>()?.Add(securityUser);
		}
    }
}
