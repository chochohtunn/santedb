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
using System;

namespace SanteDB.Persistence.Diagnostics.Jira.Model
{
    /// <summary>
    /// Issue fields
    /// </summary>
    [JsonObject]
    public class JiraIssueFields
    {
        /// <summary>
        /// Assignee
        /// </summary>
        [JsonProperty("assignee")]
        public String Assignee { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [JsonProperty("description")]
        public String Description { get; set; }

        /// <summary>
        /// Issue Type
        /// </summary>
        [JsonProperty("issuetype")]
        public JiraIdentifier IssueType { get; set; }

        /// <summary>
        /// Priority
        /// </summary>
        [JsonProperty("priority")]
        public JiraIdentifier Priority { get; set; }

        /// <summary>
        /// Project ID
        /// </summary>
        [JsonProperty("project")]
        public JiraKey Project { get; set; }

        /// <summary>
        /// Summary
        /// </summary>
        [JsonProperty("summary")]
        public String Summary { get; set; }

        /// <summary>
        /// Gets or sets the labels assigned to the object
        /// </summary>
        [JsonProperty("labels")]
        public String[] Labels { get; set; }
    }
}