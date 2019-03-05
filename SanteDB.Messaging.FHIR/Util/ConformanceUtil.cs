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
using SanteDB.Core;
using SanteDB.Core.Configuration;
using SanteDB.Core.Interop;
using SanteDB.Core.Services;
using SanteDB.Messaging.FHIR.Backbone;
using SanteDB.Messaging.FHIR.Handlers;
using SanteDB.Messaging.FHIR.Resources;
using SanteDB.Rest.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace SanteDB.Messaging.FHIR.Util
{
    /// <summary>
    /// Conformance utility
    /// </summary>
    public static class ConformanceUtil
    {
        // Conformance built
        private static Conformance s_conformance = null;
        // Sync lock
        private static Object s_syncLock = new object();

        // FHIR trace source
        private static TraceSource s_traceSource = new TraceSource("MARC.HI.EHRS.Messaging.FHIR");

        /// <summary>
        /// Get Conformance Statement from FHIR service
        /// </summary>
        public static Conformance GetConformanceStatement()
        {
            if (s_conformance == null)
                lock (s_syncLock)
                {
                    BuildConformanceStatement();
                    RestOperationContext.Current.OutgoingResponse.SetLastModified(DateTime.Now);
                }

            return s_conformance;
        }

        /// <summary>
        /// Build conformance statement
        /// </summary>
        private static void BuildConformanceStatement()
        {
            try
            {

                // No output of any exceptions
                Assembly entryAssembly = Assembly.GetEntryAssembly();

                // First assign the basic attributes
                s_conformance = new Conformance()
                {
                    Software = SoftwareDefinition.FromAssemblyInfo(),
                    AcceptUnknown = UnknownContentCode.None,
                    Date = DateTime.Now,
                    Description = "Automatically generated by ServiceCore FHIR framework",
                    Experimental = true,
                    FhirVersion = "3.0.1",
                    Format = new List<DataTypes.FhirCode<string>>() { "xml", "json" },
                    Implementation = new ImplementationDefinition()
                    {
                        Url = RestOperationContext.Current.IncomingRequest.Url,
                        Description = entryAssembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description
                    },
                    Name = "SVC-CORE FHIR",
                    Publisher = entryAssembly.GetCustomAttribute<AssemblyCompanyAttribute>().Company,
                    Title = $"Auto-Generated statement - {entryAssembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product} v{entryAssembly.GetName().Version} ({entryAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion})",
                    Status = PublicationStatus.Active,
                    Copyright = entryAssembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright
                };

                // Generate the rest description
                // TODO: Reflect the current WCF context and get all the types of communication supported
                s_conformance.Rest.Add(CreateRestDefinition());
                s_conformance.Text = null;
            }
            catch (Exception e)
            {
                s_traceSource.TraceEvent(TraceEventType.Error, e.HResult, "Error building conformance statement: {0}", e.Message);
                throw;
            }
        }

        /// <summary>
        /// Rest definition creator
        /// </summary>
        private static RestDefinition CreateRestDefinition()
        {
            // Security settings
            String security = null;
            //var m_masterConfig = ApplicationServiceContext.Current.GetService<IConfigurationManager>().GetSection<RestConfigurationSection>();
            //var authorizationPolicy = m_masterConfig.Services.FirstOrDefault(o => o.Name == "FHIR").Behaviors.Select(o => o.GetCustomAttribute<AuthenticationSchemeDescriptionAttribute>()).FirstOrDefault(o => o != null)?.Scheme;
            if (ApplicationServiceContext.Current.GetService<FhirMessageHandler>().Capabilities.HasFlag(ServiceEndpointCapabilities.BasicAuth))
                security = "Basic";
            if (ApplicationServiceContext.Current.GetService<FhirMessageHandler>().Capabilities.HasFlag(ServiceEndpointCapabilities.BearerAuth))
                security = "OAuth";

            var retVal = new RestDefinition()
            {
                Mode = "server",
                Documentation = "Default WCF REST Handler",
                Security = new SecurityDefinition()
                {
                    Cors = true,
                    Service = security == null ? null : new List<DataTypes.FhirCodeableConcept>() { new DataTypes.FhirCodeableConcept(new Uri("http://hl7.org/fhir/restful-security-service"), security) }
                },
                Resource = FhirResourceHandlerUtil.GetRestDefinition().ToList()
            };
            return retVal;


        }
    }
}
