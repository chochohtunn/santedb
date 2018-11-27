﻿/*
 * Copyright 2015-2018 Mohawk College of Applied Arts and Technology
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
 * User: justin
 * Date: 2018-11-23
 */
using SanteDB.Core.Security.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RestSrvr;
using RestSrvr.Attributes;
using RestSrvr.Message;
using SanteDB.Authentication.OAuth2.Configuration;
using SanteDB.Authentication.OAuth2.Model;
using SanteDB.Core;
using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Rest.Security;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Attribute;
using SanteDB.Core.Security.Audit;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace SanteDB.Authentication.OAuth2.Rest
{
    /// <summary>
    /// OAuth Token Service
    /// </summary>
    [ServiceBehavior(Name = "OAuth2")]
    public class OAuthTokenBehavior : IOAuthTokenContract
    {

        // Trace source name
        private TraceSource m_traceSource = new TraceSource(OAuthConstants.TraceSourceName);

        // OAuth configuration
        private OAuthConfigurationSection m_configuration = ApplicationServiceContext.Current.GetService<IConfigurationManager>().GetSection<OAuthConfigurationSection>();

        // Master configuration
        private SecurityConfigurationSection m_masterConfig = ApplicationServiceContext.Current.GetService<IConfigurationManager>().GetSection<SecurityConfigurationSection>();

        /// <summary>
        /// OAuth token request
        /// </summary>
        public Stream Token(NameValueCollection tokenRequest)
        {

            // Get the client application 
            IApplicationIdentityProviderService clientIdentityService = ApplicationServiceContext.Current.GetService<IApplicationIdentityProviderService>();
            IIdentityProviderService identityProvider = ApplicationServiceContext.Current.GetService<IIdentityProviderService>();

            // Only password grants
            if (tokenRequest["grant_type"] != OAuthConstants.GrantNamePassword &&
                tokenRequest["grant_type"] != OAuthConstants.GrantNameRefresh &&
                tokenRequest["grant_type"] != OAuthConstants.GrantNameClientCredentials)
                return this.CreateErrorCondition(OAuthErrorType.unsupported_grant_type, "Only 'password', 'client_credentials' or 'refresh_token' grants allowed");

            // Password grant needs well formed scope which defaults to * or all permissions
            if (tokenRequest["scope"] == null)
                tokenRequest.Add("scope", "*");
            // Validate username and password

            try
            {
                // Client principal
                IPrincipal clientPrincipal = Core.Security.AuthenticationContext.Current.Principal;
                // Client is not present so look in body
                if (clientPrincipal == null || clientPrincipal == Core.Security.AuthenticationContext.AnonymousPrincipal)
                {
                    String client_identity = tokenRequest["client_id"],
                        client_secret = tokenRequest["client_secret"];
                    if (String.IsNullOrEmpty(client_identity) || String.IsNullOrEmpty(client_secret))
                        return this.CreateErrorCondition(OAuthErrorType.invalid_client, "Missing client credentials");

                    try
                    {
                        clientPrincipal = ApplicationServiceContext.Current.GetService<IApplicationIdentityProviderService>().Authenticate(client_identity, client_secret);
                    }
                    catch (Exception e)
                    {
                        this.m_traceSource.TraceError("Error authenticating client: {0}", e.Message);
                        return this.CreateErrorCondition(OAuthErrorType.unauthorized_client, e.Message);
                    }
                }
                else if (!clientPrincipal.Identity.IsAuthenticated)
                    return this.CreateErrorCondition(OAuthErrorType.unauthorized_client, "Unauthorized Client");

                // Device principal?
                IPrincipal devicePrincipal = null;
                var authHead = RestOperationContext.Current.IncomingRequest.Headers["X-Device-Authorization"];

                // TODO: X509 Authentication 
                //if (RestOperationContext.Current.ServiceSecurityContext.AuthorizationContext.ClaimSets != null)
                //{
                //    var claimSet = OperationContext.Current.ServiceSecurityContext.AuthorizationContext.ClaimSets.OfType<System.IdentityModel.Claims.X509CertificateClaimSet>().FirstOrDefault();
                //    if (claimSet != null) // device authenticated with X509 PKI Cert
                //        devicePrincipal = ApplicationServiceContext.Current.GetService<IDeviceIdentityProviderService>().Authenticate(claimSet.X509Certificate);
                //}
                if (devicePrincipal == null && !String.IsNullOrEmpty(authHead)) // Device is authenticated using basic auth
                {
                    if (!authHead.ToLower().StartsWith("basic "))
                        throw new InvalidCastException("X-Device-Authorization must be BASIC scheme");

                    var authParts = Encoding.UTF8.GetString(Convert.FromBase64String(authHead.Substring(6).Trim())).Split(':');
                    devicePrincipal = ApplicationServiceContext.Current.GetService<IDeviceIdentityProviderService>().Authenticate(authParts[0], authParts[1]);
                }


                IPrincipal principal = null;

                // perform auth
                switch (tokenRequest["grant_type"])
                {
                    case OAuthConstants.GrantNameClientCredentials:
                        if (devicePrincipal == null)
                            throw new SecurityException("client_credentials grant requires device authentication either using X509 or X-Device-Authorization");
                        principal = clientPrincipal;
                        // Demand "Login As Service" permission
                        new PolicyPermission(System.Security.Permissions.PermissionState.Unrestricted, PermissionPolicyIdentifiers.LoginAsService, devicePrincipal).Demand();
                        break;
                    case OAuthConstants.GrantNamePassword:

                        // Validate 
                        if (String.IsNullOrWhiteSpace(tokenRequest["username"]) && String.IsNullOrWhiteSpace(tokenRequest["refresh_token"]))
                            return this.CreateErrorCondition(OAuthErrorType.invalid_request, "Invalid client grant message");

                        if (RestOperationContext.Current.IncomingRequest.Headers[OAuthConstants.TfaHeaderName] != null)
                            principal = identityProvider.Authenticate(tokenRequest["username"], tokenRequest["password"], RestOperationContext.Current.IncomingRequest.Headers[OAuthConstants.TfaHeaderName]);
                        else
                            principal = identityProvider.Authenticate(tokenRequest["username"], tokenRequest["password"]);

                        break;
                    case OAuthConstants.GrantNameRefresh:
                        var refreshToken = tokenRequest["refresh_token"];
                        var secret = Enumerable.Range(0, refreshToken.Length)
                                    .Where(x => x % 2 == 0)
                                    .Select(x => Convert.ToByte(refreshToken.Substring(x, 2), 16))
                                    .ToArray();
                        principal = (identityProvider as ISessionIdentityProviderService).Authenticate(ApplicationServiceContext.Current.GetService<ISessionProviderService>().Extend(secret));
                        break;
                    default:
                        throw new InvalidOperationException("Invalid grant type");
                }

                if (principal == null)
                    return this.CreateErrorCondition(OAuthErrorType.invalid_grant, "Invalid username or password");
                else
                {
                    return this.EstablishSession(principal, clientPrincipal, devicePrincipal, tokenRequest["scope"], this.ValidateClaims(principal));
                }
            }
            catch (AuthenticationException e)
            {

                this.m_traceSource.TraceEvent(TraceEventType.Error, e.HResult, "Error generating token: {0}", e);
                return this.CreateErrorCondition(OAuthErrorType.invalid_grant, e.Message);
            }
            catch (SecurityException e)
            {
                this.m_traceSource.TraceEvent(TraceEventType.Error, e.HResult, "Error generating token: {0}", e);
                return this.CreateErrorCondition(OAuthErrorType.invalid_grant, e.Message);
            }
            catch (Exception e)
            {
                this.m_traceSource.TraceEvent(TraceEventType.Error, e.HResult, "Error generating token: {0}", e);
                return this.CreateErrorCondition(OAuthErrorType.invalid_request, e.Message);
            }
        }

        /// <summary>
        /// Validate claims made by the requestor
        /// </summary>
        private IEnumerable<Claim> ValidateClaims(IPrincipal userPrincipal)
        {
            IPolicyDecisionService pdp = ApplicationServiceContext.Current.GetService<IPolicyDecisionService>();

            List<Claim> retVal = new List<Claim>();

            // HACK: Find a better way to make claims
            // Claims are stored as X-SanteDBACS-Claim headers
            foreach (var itm in SanteDBClaimTypes.ExtractClaims(RestOperationContext.Current.IncomingRequest.Headers))
            {

                // Claim allowed
                if (this.m_configuration.AllowedClientClaims == null ||
                    !this.m_configuration.AllowedClientClaims.Contains(itm.Type))
                    throw new SecurityException("Claim is not permitted");
                else
                {
                    // Validate the claim
                    var handler = SanteDBClaimTypes.GetHandler(itm.Type);
                    if (handler == null || handler.Validate(userPrincipal, itm.Value))
                        retVal.Add(itm);
                    else
                        throw new SecurityException($"Claim {itm.Type} failed validation");
                }
            }

            return retVal;
        }

        /// <summary>
        /// Hydrate the JWT token
        /// </summary>
        private JwtSecurityToken HydrateToken(ClaimsPrincipal claimsPrincipal, String scope, IEnumerable<Claim> additionalClaims, DateTime issued, DateTime expires)
        {
            this.m_traceSource.TraceInformation("Will create new ClaimsPrincipal based on existing principal");
            
            IRoleProviderService roleProvider = ApplicationServiceContext.Current.GetService<IRoleProviderService>();
            IPolicyInformationService pip = ApplicationServiceContext.Current.GetService<IPolicyInformationService>();

            // System claims
            List<Claim> claims = new List<Claim>()
            {
                new Claim("iss", this.m_configuration.IssuerName),
                new Claim(ClaimTypes.Name, claimsPrincipal.Identity.Name),
                new Claim("typ", claimsPrincipal.GetType().Name)
            };

            try
            {
                claims.AddRange(roleProvider.GetAllRoles(claimsPrincipal.Identity.Name).Select(r => new Claim(ClaimsIdentity.DefaultRoleClaimType, r)));
            }
            catch { }

            // Additional claims
            claims.AddRange(additionalClaims ?? new List<Claim>());

            // Get policies
            List<IPolicyInstance> oizPrincipalPolicies = new List<IPolicyInstance>();
            foreach (var pol in pip.GetActivePolicies(claimsPrincipal).GroupBy(o => o.Policy.Oid))
                oizPrincipalPolicies.Add(pol.FirstOrDefault(o => (int)o.Rule == pol.Min(r => (int)r.Rule)));

            // Scopes user is allowed to access
            claims.AddRange(oizPrincipalPolicies.Where(o => o.Rule == PolicyGrantType.Grant).Select(o => new Claim(SanteDBClaimTypes.SanteDBScopeClaim, o.Policy.Oid)));

            // Add grant if not exists
            if ((claimsPrincipal)?.FindFirst(ClaimTypes.Actor)?.Value == UserClassKeys.HumanUser.ToString())
            {
                claims.AddRange(new Claim[]
                    {
                    //new Claim(ClaimTypes.AuthenticationInstant, issued.ToString("o")), 
                    new Claim(ClaimTypes.AuthenticationMethod, "OAuth2"),
                    new Claim(SanteDBClaimTypes.SanteDBApplicationIdentifierClaim, claimsPrincipal?.Identities.OfType<Core.Security.ApplicationIdentity>().FirstOrDefault()?.FindFirst(ClaimTypes.Sid)?.Value)
                    });

                if (claimsPrincipal.Identities.OfType<DeviceIdentity>().Any())
                    claims.Add(new Claim(SanteDBClaimTypes.SanteDBDeviceIdentifierClaim, claimsPrincipal.Identities.OfType<DeviceIdentity>().FirstOrDefault().FindFirst(ClaimTypes.Sid)?.Value));


                // Is the user elevated? If so, add claims for those policies in scope
                if (claims.FirstOrDefault(o => o.Type == SanteDBClaimTypes.SanteDBOverrideClaim)?.Value?.ToLower() == "true")
                {
                    try
                    {
                        // 1. SCOPE must exist 
                        if (String.IsNullOrEmpty(scope))
                            throw new InvalidOperationException("Override requires scope");
                        // 2. POU must exist
                        if (!claims.Exists(c => c.Type == SanteDBClaimTypes.XspaPurposeOfUseClaim))
                            throw new InvalidOperationException("Override required purpose of use");
                        // 3. Person must have override permission
                        new PolicyPermission(System.Security.Permissions.PermissionState.Unrestricted, PermissionPolicyIdentifiers.OverridePolicyPermission, claimsPrincipal).Demand();
                        // Add elevation objects as GRANT to this session
                        claims.AddRange(oizPrincipalPolicies.Where(o => o.Rule == PolicyGrantType.Elevate).Select(o => new Claim(SanteDBClaimTypes.SanteDBScopeClaim, o.Policy.Oid)));

                        // Audit override
                        AuditUtil.AuditOverride(claimsPrincipal, claims.Where(c => c.Type == SanteDBClaimTypes.XspaPurposeOfUseClaim).FirstOrDefault().Value, scope.Split(';'), true);
                    }
                    catch (Exception e)
                    {
                        AuditUtil.AuditOverride(claimsPrincipal, claims.Where(c => c.Type == SanteDBClaimTypes.XspaPurposeOfUseClaim).FirstOrDefault().Value, scope.Split(';'), false);
                        throw;
                    }
                }

                // Now restrict down to claimed scope
                if (scope != "*")
                {
                    var scopes = scope.Split(';');
                    foreach (var s in scopes)
                        new PolicyPermission(System.Security.Permissions.PermissionState.Unrestricted, s, claimsPrincipal).Demand(); // Ensure that scope is granted
                    claims.RemoveAll(o => o.Type == SanteDBClaimTypes.SanteDBScopeClaim && !scopes.Contains(o.Value));
                }

                // Add Email address from idp
                claims.AddRange(claimsPrincipal.Claims.Where(o => o.Type == ClaimTypes.Email));
                var tel = claimsPrincipal.Claims.FirstOrDefault(o => o.Type == ClaimTypes.MobilePhone)?.Value;
                if (!String.IsNullOrEmpty(tel))
                    claims.Add(new Claim("tel", tel));
            }
            else
            {
                claims.AddRange(new Claim[]
                   {
                    //new Claim(ClaimTypes.AuthenticationInstant, issued.ToString("o")), 
                    new Claim(ClaimTypes.AuthenticationMethod, "OAuth2"),
                    new Claim(SanteDBClaimTypes.SanteDBApplicationIdentifierClaim, claimsPrincipal.Identities.OfType<Core.Security.ApplicationIdentity>().FirstOrDefault()?.FindFirst(ClaimTypes.Sid).Value),
                    new Claim(SanteDBClaimTypes.SanteDBDeviceIdentifierClaim, claimsPrincipal.Identities.OfType<Core.Security.DeviceIdentity>().FirstOrDefault()?.FindFirst(ClaimTypes.Sid)?.Value)
                   });
            }

            // Name identifier
            claims.AddRange((claimsPrincipal).Claims.Where(o => o.Type == ClaimTypes.NameIdentifier));

            // Find the nameid
            var nameId = claims.Find(o => o.Type == ClaimTypes.NameIdentifier);
            if (nameId != null)
            {
                claims.Remove(nameId);
                claims.Add(new Claim("sub", nameId.Value));
            }

            claims.RemoveAll(o => String.IsNullOrEmpty(o.Value));

            SigningCredentials credentials = this.CreateSigningCredentials();

            // Generate security token            
            var jwt = new JwtSecurityToken(
                signingCredentials: credentials,
                claims: claims,
                issuer: this.m_configuration.IssuerName,
                notBefore: issued,
                expires: expires
            );

            return jwt;

        }

        /// <summary>
        /// Create a token response
        /// </summary>
        private Stream EstablishSession(IPrincipal oizPrincipal, IPrincipal clientPrincipal, IPrincipal devicePrincipal, String scope, IEnumerable<Claim> additionalClaims)
        {
            var claimsPrincipal = oizPrincipal as ClaimsPrincipal;
            if (clientPrincipal is ClaimsPrincipal && !claimsPrincipal.Identities.OfType<Core.Security.ApplicationIdentity>().Any(o => o.Name == clientPrincipal.Identity.Name))
                claimsPrincipal.AddIdentity(clientPrincipal.Identity as ClaimsIdentity);
            if (devicePrincipal is ClaimsPrincipal && !claimsPrincipal.Identities.OfType<DeviceIdentity>().Any(o => o.Name == devicePrincipal.Identity.Name))
                claimsPrincipal.AddIdentity(devicePrincipal.Identity as ClaimsIdentity);

            // TODO: Add configuration for expiry
            DateTime issued = DateTime.Parse((claimsPrincipal)?.FindFirst(ClaimTypes.AuthenticationInstant)?.Value ?? DateTime.Now.ToString("o")),
                expires = DateTime.Now.Add(this.m_configuration.ValidityTime);
            String aud =
                RestOperationContext.Current.IncomingRequest.Headers["X-Forwarded-For"] ??
                RestOperationContext.Current.IncomingRequest.RemoteEndPoint.Address.ToString();

            var jwt = this.HydrateToken(claimsPrincipal, scope, additionalClaims, issued, expires);

            // Establish the session
            ISessionProviderService isp = ApplicationServiceContext.Current.GetService<ISessionProviderService>();
            var session = isp.Establish(new ClaimsPrincipal(claimsPrincipal.Identities), expires, aud);

            string refreshToken = null, sessionId = null;
            if (session != null)
            {
                sessionId = BitConverter.ToString(session.Id).Replace("-", "");
                (jwt.Claims as List<Claim>).Add(new Claim("jti", sessionId));
                refreshToken = BitConverter.ToString(session.RefreshToken).Replace("-", "");
            }

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            RestOperationContext.Current.OutgoingResponse.ContentType = "application/json";

            OAuthTokenResponse response = null;
            if (this.m_configuration.TokenType == OAuthConstants.BearerTokenType &&
                session != null)
                response = new OAuthTokenResponse()
                {
                    TokenType = OAuthConstants.BearerTokenType,
                    AccessToken = sessionId,
                    IdentityToken = handler.WriteToken(jwt),
                    ExpiresIn = (int)(expires.Subtract(DateTime.Now)).TotalMilliseconds,
                    RefreshToken = refreshToken // TODO: Need to write a SessionProvider for this so we can keep track of refresh tokens 
                };
            else
                response = new OAuthTokenResponse()
                {
                    TokenType = OAuthConstants.JwtTokenType,
                    AccessToken = handler.WriteToken(jwt),
                    ExpiresIn = (int)(expires.Subtract(DateTime.Now)).TotalMilliseconds,
                    RefreshToken = refreshToken // TODO: Need to write a SessionProvider for this so we can keep track of refresh tokens 
                };

            return this.CreateResponse(response);
        }

        /// <summary>
        /// Create signing credentials
        /// </summary>
        private SigningCredentials CreateSigningCredentials()
        {
            SigningCredentials retVal = null;
            // Signing credentials
            if (this.m_masterConfig.Signatures.Algorithm == SignatureAlgorithm.RS256)
            {
                var cert = X509CertificateUtils.FindCertificate(this.m_masterConfig.Signatures.FindType, this.m_masterConfig.Signatures.StoreLocation, this.m_masterConfig.Signatures.StoreName, this.m_masterConfig.Signatures.FindValue);
                if (cert == null)
                    throw new SecurityException("Cannot find certificate to sign JWT tokens!");
                retVal = new X509SigningCredentials(cert);

            }
            else if (this.m_masterConfig.Signatures.Algorithm == SignatureAlgorithm.HS256)
            {
                retVal = new SigningCredentials(
                    new InMemorySymmetricSecurityKey(this.m_masterConfig.Signatures.Secret),
                    "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256",
                    "http://www.w3.org/2001/04/xmlenc#sha256",
                    new SecurityKeyIdentifier(new NamedKeySecurityKeyIdentifierClause("keyid", "0"))
                );
            }
            else
                throw new SecurityException("Invalid signing configuration");
            return retVal;
        }

        /// <summary>
        /// Create error condition
        /// </summary>
        private Stream CreateErrorCondition(OAuthErrorType errorType, String message)
        {
            this.m_traceSource.TraceEvent(TraceEventType.Error, (int)errorType, message);
            RestOperationContext.Current.OutgoingResponse.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
            OAuthError err = new OAuthError()
            {
                Error = errorType,
                ErrorDescription = message
            };
            return this.CreateResponse(err);
        }

        /// <summary>
        /// Create response
        /// </summary>
        private Stream CreateResponse(Object response)
        {
            var settings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            settings.Converters.Add(new StringEnumConverter());
            String result = JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.Indented, settings);
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(result));
            RestOperationContext.Current.OutgoingResponse.ContentType = "application/json";
            return ms;
        }

        /// <summary>
        /// Get the specified session information
        /// </summary>
        public Stream Session()
        {
            new TokenAuthorizationAccessBehavior().Apply(new RestRequestMessage(RestOperationContext.Current.IncomingRequest)); ;
            var principal = Core.Security.AuthenticationContext.Current.Principal as ClaimsPrincipal;

            if (principal != null)
            {

                if (principal.Identity.Name == Core.Security.AuthenticationContext.AnonymousPrincipal.Identity.Name)
                    return this.CreateResponse(new OAuthError()
                    {
                        Error = OAuthErrorType.invalid_request,
                        ErrorDescription = "No Such Session"
                    });
                else
                {
                    DateTime notBefore = DateTime.Parse(principal.FindFirst(ClaimTypes.AuthenticationInstant).Value), notAfter = DateTime.Parse(principal.FindFirst(ClaimTypes.Expiration).Value);
                    var jwt = this.HydrateToken(principal, principal.FindFirst(SanteDBClaimTypes.SanteDBScopeClaim)?.Value ?? "*", null, notBefore, notAfter);
                    return this.CreateResponse(new OAuthTokenResponse()
                    {
                        AccessToken = RestOperationContext.Current.IncomingRequest.Headers["Authorization"].Split(' ')[1],
                        IdentityToken = new JwtSecurityTokenHandler().WriteToken(jwt),
                        ExpiresIn = (int)(notAfter).Subtract(DateTime.Now).TotalMilliseconds,
                        TokenType = this.m_configuration.TokenType
                    });
                }
            }
            else
            {
                return this.CreateResponse(new OAuthError()
                {
                    Error = OAuthErrorType.invalid_request,
                    ErrorDescription = "No Such Session"
                });
            }
        }
    }
}
