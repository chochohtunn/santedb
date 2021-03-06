﻿/*
 * Based on OpenIZ - Based on OpenIZ, Copyright (C) 2015 - 2019 Mohawk College of Applied Arts and Technology
 * Portions Copyright 2019-2020, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE)
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
 * User: fyfej (Justin Fyfe)
 * Date: 2019-11-27
 */
using SanteDB.Core.Http;
using SanteDB.Core.Http.Description;
using SanteDB.Tools.AdminConsole.Shell;
using System;
using System.Collections.Generic;

namespace SanteDB.Tools.AdminConsole.Security
{
    /// <summary>
    /// Rest Client Security Description
    /// </summary>
    public class SecurityConfigurationDescription : IRestClientSecurityDescription
    {

        // Cert validator
        private ICertificateValidator m_certificateValidator = new ConsoleCertificateValidator();

        /// <summary>
        /// Authentication realm
        /// </summary>
        public string AuthRealm
        {
            get
            {
                return ApplicationContext.Current.RealmId;
            }
        }

        /// <summary>
        /// Certificate validator
        /// </summary>
        public ICertificateValidator CertificateValidator
        {
            get
            {
                return this.m_certificateValidator;
            }
        }

        /// <summary>
        /// Gets the client certificate
        /// </summary>
        public IRestClientCertificateDescription ClientCertificate
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the credential provider
        /// </summary>
        public ICredentialProvider CredentialProvider
        {
            get; set;
        }

        /// <summary>
        /// Security scheme
        /// </summary>
        public SecurityScheme Mode
        {
            get; set;
        }

        /// <summary>
        /// Preemtive authentication?
        /// </summary>
        public bool PreemptiveAuthentication
        {
            get; set;
        }
    }

    /// <summary>
    /// Certificate validator
    /// </summary>
    internal class ConsoleCertificateValidator : ICertificateValidator
    {

        private static HashSet<object> m_trustedCerts = new HashSet<object>();

        /// <summary>
        /// Validate certificate
        /// </summary>
        public bool ValidateCertificate(object certificate, object chain)
        {
            if (m_trustedCerts.Contains(certificate.ToString())) return true;
            String response = String.Empty;
            try
            {
                Console.ForegroundColor = ConsoleColor.Red;
                while (response != "y" && response != "n" && response != "s")
                {
                    Console.WriteLine("Certificate {0} presented by server is invalid.", certificate.ToString());
                    Console.Write("Trust this certificate? ([y]es/[n]o/[s]ession):");
                    response = Console.ReadLine();
                }
            }
            finally
            {
                Console.ResetColor();
            }

            if (response == "s")
            {
                m_trustedCerts.Add(certificate.ToString());
                return true;
            }
            else
                return response == "y";
        }
    }
}
