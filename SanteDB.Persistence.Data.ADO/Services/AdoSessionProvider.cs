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
using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using SanteDB.Persistence.Data.ADO.Configuration;
using SanteDB.Persistence.Data.ADO.Data.Model.Security;
using SanteDB.Persistence.Data.ADO.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Security;

using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace SanteDB.Persistence.Data.ADO.Services
{
    /// <summary>
    /// Represents a session provider for ADO based sessions
    /// </summary>
    [ServiceProvider("ADO.NET Session Storage")]
    public class AdoSessionProvider : ISessionProviderService
    {
        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => "ADO.NET Session Storage Provider";

        // Sync lock
        private Object m_syncLock = new object();

        // Trace source
        private Tracer m_traceSource = new Tracer(AdoDataConstants.IdentityTraceSourceName);

        // Configuration
        private AdoPersistenceConfigurationSection m_configuration = ApplicationServiceContext.Current.GetService<IConfigurationManager>().GetSection<AdoPersistenceConfigurationSection>();

        // Session cache
        private Dictionary<Guid, DbSession> m_sessionCache = new Dictionary<Guid, DbSession>();

        // Session lookups
        private Int32 m_sessionLookups = 0;

        public event EventHandler<SessionEstablishedEventArgs> Established;


        /// <summary>
        /// Create and register a refresh token for the specified principal
        /// </summary>
        public byte[] CreateRefreshToken()
        {
            // First we shall set the refresh claim
            return Guid.NewGuid().ToByteArray();
        }

        /// <summary>
        /// Establish the session
        /// </summary>
        /// <param name="principal">The security principal for which the session is being created</param>
        /// <param name="expiry">The expiration of the session</param>
        /// <param name="aud">The audience of the session</param>
        /// <returns>A constructed <see cref="global::ThisAssembly:AdoSession"/></returns>
        public ISession Establish(IPrincipal principal, DateTimeOffset expiry, String remoteEp)
        {
            // Validate the parameters
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));
            else if (!principal.Identity.IsAuthenticated)
                throw new InvalidOperationException("Cannot create a session for a non-authenticated principal");
            else if (!(principal is IClaimsPrincipal))
                throw new ArgumentException("Principal must be ClaimsPrincipal", nameof(principal));

            var cprincipal = principal as IClaimsPrincipal;

            try
            {
               
                using (var context = this.m_configuration.Provider.GetWriteConnection())
                {
                    context.Open();
                    var refreshToken = this.CreateRefreshToken();

                    var applicationKey = cprincipal.Identities.OfType<Core.Security.ApplicationIdentity>()?.FirstOrDefault()?.FindFirst(SanteDBClaimTypes.Sid)?.Value ??
                        cprincipal.FindFirst(SanteDBClaimTypes.SanteDBApplicationIdentifierClaim)?.Value;
                    var deviceKey = cprincipal.Identities.OfType<Core.Security.DeviceIdentity>()?.FirstOrDefault()?.FindFirst(SanteDBClaimTypes.Sid)?.Value ??
                        cprincipal.FindFirst(SanteDBClaimTypes.SanteDBDeviceIdentifierClaim)?.Value;
                    var userKey = cprincipal.FindFirst(SanteDBClaimTypes.Sid).Value;

                    var dbSession = new DbSession()
                    {
                        DeviceKey = deviceKey != null ? (Guid?)Guid.Parse(deviceKey) : null,
                        ApplicationKey = Guid.Parse(applicationKey),
                        UserKey = userKey != null && userKey != deviceKey ? (Guid?)Guid.Parse(userKey) : null,
                        NotBefore = DateTimeOffset.Now,
                        NotAfter = expiry,
                        RefreshExpiration = expiry.AddMinutes(10),
                        RemoteEndpoint = remoteEp,
                        RefreshToken = ApplicationServiceContext.Current.GetService<IPasswordHashingService>().ComputeHash(BitConverter.ToString(refreshToken).Replace("-", ""))
                    };

                    if (dbSession.ApplicationKey == dbSession.UserKey) // SID == Application = Application Grant
                        dbSession.UserKey = Guid.Empty;

                    dbSession = context.Insert(dbSession);

                    var signingService = ApplicationServiceContext.Current.GetService<IDataSigningService>();

                    if (signingService == null)
                    {
                        this.m_traceSource.TraceWarning("No IDataSigningService provided. Session data will be unsigned!");
                        var session = new AdoSecuritySession(dbSession.Key, dbSession.Key.ToByteArray(), refreshToken, dbSession.NotBefore, dbSession.NotAfter);
                        this.Established?.Invoke(this, new SessionEstablishedEventArgs(principal, session, true));
                        return session;
                    }
                    else
                    {
                        var signedToken = dbSession.Key.ToByteArray().Concat(signingService.SignData(dbSession.Key.ToByteArray())).ToArray();
                        var signedRefresh = refreshToken.Concat(signingService.SignData(refreshToken)).ToArray();

                        var session = new AdoSecuritySession(dbSession.Key, signedToken, signedRefresh, dbSession.NotBefore, dbSession.NotAfter);
                        this.Established?.Invoke(this, new SessionEstablishedEventArgs(principal, session, true));
                        return session;
                    }
                }
            }
            catch (Exception e)
            {
                this.m_traceSource.TraceError("Error establishing session: {0}", e.Message);
                this.Established?.Invoke(this, new SessionEstablishedEventArgs(principal, null, false));

                throw;
            }
        }

        /// <summary>
        /// Extend the session 
        /// </summary>
        /// <param name="refreshToken">The signed session token to be refreshed</param>
        /// <returns>The session that was extended</returns>
        public ISession Extend(byte[] refreshToken)
        {
            // Validate the parameters
            if (refreshToken == null)
                throw new ArgumentNullException(nameof(refreshToken));

            IDbTransaction tx = null;

            using (var context = this.m_configuration.Provider.GetWriteConnection())
            {
                try
                {

                    context.Open();

                    tx = context.BeginTransaction();

                    var signingService = ApplicationServiceContext.Current.GetService<IDataSigningService>();
                    if (signingService == null)
                    {
                        this.m_traceSource.TraceWarning("No IDataSigningService provided. Digital signatures will not be verified");
                    }
                    else if (!signingService.Verify(refreshToken.Take(16).ToArray(), refreshToken.Skip(16).ToArray()))
                        throw new SecurityException("Refresh token appears to have been tampered with");

                    // Get the session to be extended
                    var qToken = BitConverter.ToString(refreshToken.Take(16).ToArray()).Replace("-", "");
                    qToken = ApplicationServiceContext.Current.GetService<IPasswordHashingService>().ComputeHash(qToken);
                    var dbSession = context.SingleOrDefault<DbSession>(o => o.RefreshToken == qToken && o.RefreshExpiration > DateTimeOffset.Now);
                    if (dbSession == null)
                        throw new FileNotFoundException(BitConverter.ToString(refreshToken));

                    // Get rid of the old session
                    context.Delete(dbSession);

                    // Generate a new session for this user
                    dbSession.Key = Guid.Empty;
                    refreshToken = this.CreateRefreshToken();
                    dbSession.RefreshToken = ApplicationServiceContext.Current.GetService<IPasswordHashingService>().ComputeHash(BitConverter.ToString(refreshToken).Replace("-", ""));
                    dbSession.NotAfter = DateTimeOffset.Now + (dbSession.NotAfter - dbSession.NotBefore); // Extend for original time
                    dbSession.NotBefore = DateTimeOffset.Now;
                    dbSession.RefreshExpiration = dbSession.NotAfter.AddMinutes(10);

                    // Save
                    context.Insert(dbSession);

                    tx.Commit();

                    if (signingService == null)
                    {
                        this.m_traceSource.TraceWarning("No IDataSigningService provided. Session data will be unsigned!");
                        return new AdoSecuritySession(dbSession.Key, dbSession.Key.ToByteArray(), refreshToken, dbSession.NotBefore, dbSession.NotAfter);
                    }
                    else
                    {
                        var signedToken = dbSession.Key.ToByteArray().Concat(signingService.SignData(dbSession.Key.ToByteArray())).ToArray();
                        var signedRefresh = refreshToken.Concat(signingService.SignData(refreshToken)).ToArray();
                        return new AdoSecuritySession(dbSession.Key, signedToken, signedRefresh, dbSession.NotBefore, dbSession.NotAfter);
                    }

                }
                catch (Exception e)
                {
                    tx?.Rollback();
                    this.m_traceSource.TraceError("Error getting session: {0}", e.Message);
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets the specified session if valid from a signed session token
        /// </summary>
        /// <param name="sessionToken">The session token to retrieve the session for</param>
        /// <returns>The fetched session token</returns>
        public ISession Get(byte[] sessionToken)
        {
            // Validate the parameters
            if (sessionToken == null)
                throw new ArgumentNullException(nameof(sessionToken));

            try
            {
                using (var context = this.m_configuration.Provider.GetReadonlyConnection())
                {
                    context.Open();

                    var signingService = ApplicationServiceContext.Current.GetService<IDataSigningService>();
                    if (signingService == null)
                        this.m_traceSource.TraceWarning("No IDataSigingService registered. Session data will not be verified");
                    else if (!signingService.Verify(sessionToken.Take(16).ToArray(), sessionToken.Skip(16).ToArray()))
                        throw new SecurityException("Session token appears to have been tampered with");

                    var sessionId = new Guid(sessionToken.Take(16).ToArray());

                    // Check the cache
                    DbSession dbSession = null;
                    if (!this.m_sessionCache.TryGetValue(sessionId, out dbSession))
                    {
                        dbSession = context.SingleOrDefault<DbSession>(o => o.Key == sessionId && o.NotAfter > DateTimeOffset.Now);
                        if (dbSession == null)
                            throw new FileNotFoundException(BitConverter.ToString(sessionToken));
                        else lock (this.m_syncLock)
                            {
                                if (!this.m_sessionCache.ContainsKey(sessionId))
                                    this.m_sessionCache.Add(sessionId, dbSession);
                                else
                                    this.m_sessionCache[sessionId] = dbSession;
                            }
                    }

                    // TODO: Write a timer job for this
                    lock (this.m_syncLock)
                    {
                        this.m_sessionLookups++;
                        if (this.m_sessionLookups > 10000) // Clean up  {
                        {
                            this.m_sessionLookups = 0;
                            this.m_traceSource.TraceInfo("Cleaning expired sessions from cache");
                            var keyIds = this.m_sessionCache.Where(s => s.Value.NotAfter <= DateTimeOffset.Now).Select(s => s.Key).ToList();
                            foreach (var kid in keyIds)
                                this.m_sessionCache.Remove(kid);
                        }
                    }

                    return new AdoSecuritySession(dbSession.Key, sessionToken, null, dbSession.NotBefore, dbSession.NotAfter);
                }
            }
            catch (Exception e)
            {
                this.m_traceSource.TraceError("Error getting session: {0}", e.Message);
                throw;
            }
        }
    }
}
