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
using System.Xml.Serialization;

namespace SanteDB.Reporting.Core.Configuration
{
    /// <summary>
    /// Represents a username and password credential.
    /// </summary>
    [XmlType(nameof(UsernamePasswordCredential), Namespace = "http://santedb.org/reporting")]
	public class UsernamePasswordCredential : CredentialBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="UsernamePasswordCredential"/> class.
		/// </summary>
		public UsernamePasswordCredential()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UsernamePasswordCredential"/> class.
		/// </summary>
		/// <param name="username">The username.</param>
		/// <param name="password">The password.</param>
		public UsernamePasswordCredential(string username, string password)
		{
			this.Username = username;
			this.Password = password;
		}

		/// <summary>
		/// Gets or sets the password.
		/// </summary>
		/// <value>The password.</value>
		[XmlAttribute("password")]
		public string Password { get; set; }

		/// <summary>
		/// Gets or sets the username.
		/// </summary>
		/// <value>The username.</value>
		[XmlAttribute("username")]
		public string Username { get; set; }
	}
}