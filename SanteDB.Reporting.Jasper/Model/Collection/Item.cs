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

namespace SanteDB.Reporting.Jasper.Model.Collection
{
    /// <summary>
    /// Represents an item.
    /// </summary>
    [XmlType("item")]
	public class Item
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Item"/> class.
		/// </summary>
		public Item()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Item"/> class.
		/// </summary>
		/// <param name="label">The label.</param>
		/// <param name="value">The value.</param>
		public Item(string label, string value)
		{
			this.Label = label;
			this.Value = value;
		}

		/// <summary>
		/// Gets or sets the label.
		/// </summary>
		/// <value>The label.</value>
		[XmlElement("label")]
		public string Label { get; set; }

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>The value.</value>
		[XmlElement("value")]
		public string Value { get; set; }
	}
}