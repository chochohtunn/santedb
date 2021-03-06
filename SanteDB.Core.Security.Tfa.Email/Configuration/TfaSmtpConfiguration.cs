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
using System;
using System.Xml.Serialization;

namespace SanteDB.Core.Security.Tfa.Email.Configuration
{
    /// <summary>
    /// Configuration for SMTP
    /// </summary>
    [XmlType(nameof(TfaSmtpConfiguration), Namespace = "http://santedb.org/configuration")]
    public class TfaSmtpConfiguration
	{

        /// <summary>
        /// Create new smtp configuration
        /// </summary>
        public TfaSmtpConfiguration()
        {

        }
		/// <summary>
		/// SMTP configuration
		/// </summary>
		public TfaSmtpConfiguration(Uri server, String userName, String password, bool ssl, String from)
		{
			this.ServerXml = server.ToString();
			this.Username = userName;
			this.Password = password;
			this.Ssl = ssl;
            this.From = from;
		}

        /// <summary>
        /// Gets the from address
        /// </summary>
        [XmlAttribute("from")]
        public String From { get; set; }

        /// <summary>
        /// Gets the password
        /// </summary>
        [XmlAttribute("password")]
        public string Password { get; set; }

		/// <summary>
		/// Gets the SMTP server
		/// </summary>
        [XmlAttribute("server")]
        public String ServerXml { get; set; }

        /// <summary>
        /// Get the server
        /// </summary>
        [XmlIgnore]
        public Uri Server
        {
            get
            {
                return new Uri(this.ServerXml);
            }
        }

		/// <summary>
		/// Get the SSL setting
		/// </summary>
        [XmlAttribute("ssl")]
		public bool Ssl { get; set; }

		/// <summary>
		/// Gets the username for connecting to the server
		/// </summary>
        [XmlAttribute("username")]
		public string Username { get; set; }
	}
}