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
using SanteDB.Core.Services.Impl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration
{

    /// <summary>
    /// Identifies the policy enforcement exception
    /// </summary>
    [XmlType(nameof(PolicyEnforcementExemptionPolicy), Namespace = "http://santedb.org/configuration")]
    public enum PolicyEnforcementExemptionPolicy
    {
        /// <summary>
        /// No exemptions
        /// </summary>
        [XmlEnum("none")]
        NoExemptions = 0,
        /// <summary>
        /// Devices exempt
        /// </summary>
        [XmlEnum("devices")]
        DevicePrincipalsExempt = 0x1,
        /// <summary>
        /// Users exempt
        /// </summary>
        [XmlEnum("humans")]
        UserPrincipalsExempt = 0x2,
        /// <summary>
        /// Devices and humans are exempt
        /// </summary>
        [XmlEnum("all")]
        AllExempt = DevicePrincipalsExempt | UserPrincipalsExempt
    }

    /// <summary>
    /// SanteDB Security configuration
    /// </summary>
    [XmlType(nameof(SecurityConfigurationSection), Namespace = "http://santedb.org/configuration")]
    public class SecurityConfigurationSection : IConfigurationSection
    {

        /// <summary>
        /// Security configuration section
        /// </summary>
        public SecurityConfigurationSection()
        {
            this.Signatures = new List<SecuritySignatureConfiguration>();
            this.PasswordRegex = RegexPasswordValidator.DefaultPasswordPattern;
            this.TrustedCertificates = new ObservableCollection<string>();
        }

        /// <summary>
        /// Password regex
        /// </summary>
        [XmlAttribute("passwordRegex")]
        [DisplayName("Password Regex")]
        [Description("Identifies the password regular expression")]
        public string PasswordRegex { get; set; }

        /// <summary>
        /// Policy enforcement policy
        /// </summary>
        [XmlAttribute("pepExemptionPolicy")]
        [DisplayName("PEP Exemption Policy")]
        [Description("Identifies the policy enforcement exception." +
            "When set, certain types of security principals will not be subject to PEP rules." + 
            "DevicePrincipalsExempt indicates that userless principals should not be subject to PEP enforcement" + 
            "UserPrincipalsExempt indicates that user principals should be should not be subject to PEP enforcement")]
        public PolicyEnforcementExemptionPolicy PepExemptionPolicy { get; set; }

        /// <summary>
        /// Signature configuration
        /// </summary>
        [XmlArray("signingKeys"), XmlArrayItem("add")]
        [Description("Describes the algorithm and key for signing data originating from this server")]
        [DisplayName("Data Signatures")]
        public List<SecuritySignatureConfiguration> Signatures { get; set; }

        /// <summary>
        /// Trusted publishers
        /// </summary>
        [XmlArray("trustedCertificates"), XmlArrayItem("add")]
        [DisplayName("Trusted Certificates")]
        [Description("Individual X.509 certificate thumbprints to trust")]
        public ObservableCollection<string> TrustedCertificates { get; set; }

        /// <summary>
        /// Maximum invalid logins
        /// </summary>
        [XmlElement("maxInvalidLogins")]
        [DisplayName("Maximum Invalid Logins")]
        [Description("The maximum invalid logins before an account is locked")]
        public int? MaxInvalidLogins { get; set; }

        /// <summary>
        /// Maximum invalid logins
        /// </summary>
        [XmlElement("passwordAging")]
        [DisplayName("Password Age")]
        [Description("The maximum password age")]
        public TimeSpan? MaxPasswordAge { get; set; }
    }
}