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
using System.ComponentModel;
using System.Xml.Serialization;

namespace SanteDB.Messaging.FHIR.Backbone
{
    /// <summary>
    /// Filter operators
    /// </summary>
    [XmlType("FilterOperator", Namespace = "http://hl7.org/fhir")]
    [FhirValueSet(Uri = "http://hl7.org/fhir/ValueSet/filter-operator")]
    public enum FilterOperator
    {
        /// <summary>
        /// A = B
        /// </summary>
        [XmlEnum("=")]
        Eq,
        /// <summary>
        /// A is a B
        /// </summary>
        [XmlEnum("is-a")]
        IsA,
        /// <summary>
        /// A is not a B
        /// </summary>
        [XmlEnum("is-not-a")]
        IsNotA,
        /// <summary>
        /// A matches regex B
        /// </summary>
        [XmlEnum("regex")]
        Regex,
        /// <summary>
        /// A appears in B
        /// </summary>
        [XmlEnum("in")]
        In,
        /// <summary>
        /// A does not appear in B
        /// </summary>
        [XmlEnum("not-in")]
        NotIn
    }

    /// <summary>
    /// Composition filter definition
    /// </summary>
    [XmlType("ValueSet.Compose.Include.Filter", Namespace = "http://hl7.org/fhir")]
    public class ComposeFilterDefinition : BackboneElement
    {
        /// <summary>
        /// A property defined by the code system
        /// </summary>
        [XmlElement("property")]
        [Description("A property defined by the code system")]
        [FhirElement(MinOccurs = 1)]
        public FhirCode<String> Property { get; set; }

        /// <summary>
        /// Gets or sets the operator
        /// </summary>
        [XmlElement("op")]
        [Description("Filter operator applied to the property")]
        [FhirElement(MinOccurs = 1)]
        public FhirCode<FilterOperator> Op { get; set; }

        /// <summary>
        /// Gets or sets the code form the system or regex criteria
        /// </summary>
        [XmlElement("value")]
        [Description("Code from the system or regex criteria")]
        [FhirElement(MinOccurs = 1)]
        public FhirCode<String> Value { get; set; }

    }
}
