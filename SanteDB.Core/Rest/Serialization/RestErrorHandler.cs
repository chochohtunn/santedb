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
using RestSrvr;
using RestSrvr.Exceptions;
using RestSrvr.Message;
using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Rest.Security;
using SanteDB.Core.Security.Audit;
using SanteDB.Core.Services;
using SanteDB.Rest.Common.Fault;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Security.Authentication;

namespace SanteDB.Core.Rest.Serialization
{
    /// <summary>
    /// Error handler
    /// </summary>
    public class RestErrorHandler : IServiceErrorHandler
    {

        // Error tracer
        private Tracer m_traceSource = Tracer.GetTracer(typeof(RestErrorHandler));
        private ClaimsAuthorizationConfigurationSection m_configuration = ApplicationServiceContext.Current.GetService<IConfigurationManager>().GetSection<ClaimsAuthorizationConfigurationSection>();

        /// <summary>
        /// Handle error
        /// </summary>
        public bool HandleError(Exception error)
        {
            return true;
        }

        /// <summary>
        /// Provide fault
        /// </summary>
        public bool ProvideFault(Exception error, RestResponseMessage faultMessage)
        {

            this.m_traceSource.TraceError("Error on REST pipeline: {0}", error);
            var uriMatched = RestOperationContext.Current.IncomingRequest.Url;

            var fault = new RestServiceFault(error);

            // Formulate appropriate response
            if (error is DomainStateException)
                faultMessage.StatusCode = (int)System.Net.HttpStatusCode.ServiceUnavailable;
            else if (error is PolicyViolationException)
            {
                var pve = error as PolicyViolationException;
                if (pve.PolicyDecision == PolicyGrantType.Elevate)
                {
                    // Ask the user to elevate themselves
                    faultMessage.StatusCode = 401;
                    var authHeader = $"{(RestOperationContext.Current.AppliedPolicies.OfType<BasicAuthorizationAccessBehavior>().Any() ? "Basic" : "Bearer")} realm=\"{RestOperationContext.Current.IncomingRequest.Url.Host}\" error=\"insufficient_scope\" scope=\"{pve.PolicyId}\"  error_description=\"{error.Message}\"";
                    AuditUtil.AuditRestrictedFunction(error as PolicyViolationException, uriMatched, authHeader);
                    RestOperationContext.Current.OutgoingResponse.AddHeader("WWW-Authenticate", authHeader);

                }
                else
                {
                    faultMessage.StatusCode = 403;
                    AuditUtil.AuditRestrictedFunction(error, uriMatched, "HTTP-403");
                }
            }
            else if (error is SecurityException)
            {
                faultMessage.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
            }
            else if (error is SecurityTokenException)
            {
                // TODO: Audit this
                faultMessage.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
                var authHeader = $"Bearer realm=\"{RestOperationContext.Current.IncomingRequest.Url.Host}\" error=\"invalid_token\" error_description=\"{error.Message}\"";
                AuditUtil.AuditRestrictedFunction(error as SecurityTokenException, uriMatched, authHeader);
                RestOperationContext.Current.OutgoingResponse.AddHeader("WWW-Authenticate", authHeader );
            }
            else if (error is LimitExceededException)
            {

                faultMessage.StatusCode = (int)(HttpStatusCode)429;
                faultMessage.StatusDescription = "Too Many Requests";
                faultMessage.Headers.Add("Retry-After", "1200");
            }
            else if (error is AuthenticationException)
            {
                var authHeader = $"{(RestOperationContext.Current.AppliedPolicies.OfType<BasicAuthorizationAccessBehavior>().Any() ? "Basic" : "Bearer")} realm=\"{RestOperationContext.Current.IncomingRequest.Url.Host}\" error=\"invalid_token\" error_description=\"{error.Message}\"";
                AuditUtil.AuditRestrictedFunction(error as AuthenticationException, uriMatched, authHeader);
                faultMessage.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
                RestOperationContext.Current.OutgoingResponse.AddHeader("WWW-Authenticate", authHeader);
            }
            else if (error is UnauthorizedAccessException)
            {
                AuditUtil.AuditRestrictedFunction(error, uriMatched, "HTTP-403");
                faultMessage.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
            }
            else if (error is FaultException)
                faultMessage.StatusCode = (int)(error as FaultException).StatusCode;
            else if (error is Newtonsoft.Json.JsonException ||
                error is System.Xml.XmlException)
                faultMessage.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
            else if (error is DuplicateKeyException || error is DuplicateNameException)
                faultMessage.StatusCode = (int)System.Net.HttpStatusCode.Conflict;
            else if (error is FileNotFoundException || error is KeyNotFoundException)
                faultMessage.StatusCode = (int)System.Net.HttpStatusCode.NotFound;
            else if (error is DomainStateException)
                faultMessage.StatusCode = (int)System.Net.HttpStatusCode.ServiceUnavailable;
            else if (error is DetectedIssueException)
                faultMessage.StatusCode = (int)(System.Net.HttpStatusCode)422;
            else if (error is NotImplementedException)
                faultMessage.StatusCode = (int)HttpStatusCode.NotImplemented;
            else if (error is NotSupportedException)
                faultMessage.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            else
                faultMessage.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;

            RestMessageDispatchFormatter.CreateFormatter(RestOperationContext.Current.ServiceEndpoint.Description.Contract.Type).SerializeResponse(faultMessage, null, fault);
            return true;
            
        }
    }
}
