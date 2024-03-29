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

namespace SanteDB.Reporting.Jasper.Model
{
    /// <summary>
    /// Represents a base resource in jasper.
    /// </summary>
    public abstract class ResourceBase
	{
		/// <summary>
		/// Gets or sets the creation time.
		/// </summary>
		/// <value>The creation time.</value>
		[XmlElement("creationDate")]
		public DateTime CreationTime { get; set; }

		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>The description.</value>
		[XmlElement("description")]
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets the label.
		/// </summary>
		/// <value>The label.</value>
		[XmlElement("label")]
		public string Label { get; set; }

		/// <summary>
		/// Gets or sets the permission mask.
		/// </summary>
		/// <value>The permission mask.</value>
		[XmlElement("permissionMask")]
		public int PermissionMask { get; set; }

		/// <summary>
		/// Gets or sets the updated time.
		/// </summary>
		/// <value>The updated time.</value>
		[XmlElement("updateDate")]
		public DateTime UpdatedTime { get; set; }

		/// <summary>
		/// Gets or sets the URI.
		/// </summary>
		/// <value>The URI.</value>
		[XmlElement("uri")]
		public string Uri { get; set; }

		/// <summary>
		/// Gets or sets the version.
		/// </summary>
		/// <value>The version.</value>
		[XmlElement("version")]
		public int Version { get; set; }
	}
}