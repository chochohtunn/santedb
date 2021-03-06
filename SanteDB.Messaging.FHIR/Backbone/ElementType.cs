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
using SanteDB.Messaging.FHIR.Attributes;
using SanteDB.Messaging.FHIR.DataTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace SanteDB.Messaging.FHIR.Backbone
{

    /// <summary>
    /// Identifies the aggregation mode of the eleemnt
    /// </summary>
    [XmlType("AggregationMode", Namespace = "http://hl7.org/fhir")]
    [FhirValueSet(Uri = "http://hl7.org/fhir/ValueSet/resource-aggregation-mode")]
    public enum AggregationMode
    {
        /// <summary>
        /// Element should be contained in containedResources
        /// </summary>
        [XmlEnum("contained")]
        Contained,
        /// <summary>
        /// Element should be referenced
        /// </summary>
        [XmlEnum("referenced")]
        Referenced,
        /// <summary>
        /// Element is bundled
        /// </summary>
        [XmlEnum("bundled")]
        Bundled
    }

    /// <summary>
    /// Data type and profile for an element
    /// </summary>
    [XmlType("ElementType", Namespace = "http://hl7.org/fhir")]
    public class ElementType : FhirElement
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ElementType()
        {
            this.Profile = new List<FhirUri>();
            this.Aggregation = new List<FhirCode<AggregationMode>>();
        }

        /// <summary>
        /// The datatype resource being profiles
        /// </summary>
        [XmlElement("code")]
        [Description("Name of datatype or resource")]
        [FhirElement(MinOccurs = 1, RemoteBinding = "http://hl7.org/fhir/ValueSet/defined-types")]
        public FhirCode<String> Code { get; set; }

        /// <summary>
        /// Profile to apply
        /// </summary>
        [XmlElement("profile")]
        [Description("Profile to apply")]
        public List<FhirUri> Profile { get; set; }

        /// <summary>
        /// Identifies the aggregation mode
        /// </summary>
        public List<FhirCode<AggregationMode>> Aggregation { get; set; }
    }
}