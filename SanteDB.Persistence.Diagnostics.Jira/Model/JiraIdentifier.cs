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
using Newtonsoft.Json;

namespace SanteDB.Persistence.Diagnostics.Jira.Model
{
    /// <summary>
    /// Represents a jira identifier
    /// </summary>
    [JsonObject]
    public class JiraIdentifier
    {

        /// <summary>
        /// Default ctor
        /// </summary>
        public JiraIdentifier()
        {

        }

        /// <summary>
        /// Creates a new identifier with specified id
        /// </summary>
        public JiraIdentifier(int id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Creates a new identifier with specified name
        /// </summary>
        public JiraIdentifier(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Represents numeric identifier
        /// </summary>
        [JsonProperty("id")]
        public int? Id { get; set; }

        /// <summary>
        /// Represents name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

    }
}