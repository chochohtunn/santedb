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
using SanteDB.Messaging.FHIR.DataTypes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace SanteDB.Messaging.FHIR.Backbone
{

    /// <summary>
    /// Allergy or intolerance severity
    /// </summary>
    [XmlType("AllergyIntoleranceSeverity", Namespace = "http://hl7.org/fhir")]
    public enum AllergyIntoleranceSeverity
    {
        /// <summary>
        /// Intolerance is severe
        /// </summary>
        [XmlEnum("severe")]
        Severe, 
        /// <summary>
        /// Intolerance is mild
        /// </summary>
        [XmlEnum("mild")]
        Mild,
        /// <summary>
        /// Intolerance is moderate
        /// </summary>
        [XmlEnum("moderate")]
        Moderate
    }

    /// <summary>
    /// Allergy or intolerance reaction
    /// </summary>
    [XmlType("AllergyIntoleranceReaction", Namespace = "http://hl7.org/fhir")]
    public class AllergyIntoleranceReaction : BackboneElement
    {

        /// <summary>
        /// Gets or sets the substance code
        /// </summary>
        [XmlElement("substance")]
        [Description("Specific substance which is responsible for the event")]
        public FhirCodeableConcept Substance { get; set; }

        /// <summary>
        /// Manifestation
        /// </summary>
        [XmlElement("manifestation")]
        [Description("How the allergy manifested itself")]
        public List<FhirCodeableConcept> Manifestation { get; set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        [XmlElement("description")]
        [Description("A textual description of the reaction")]
        public FhirString Description { get; set; }

        /// <summary>
        /// Gets or sets the onset date
        /// </summary>
        [XmlElement("onset")]
        [Description("The date of onset")]
        public FhirDateTime Onset { get; set; }

        /// <summary>
        /// The severity of the allergy reaction
        /// </summary>
        [XmlElement("severity")]
        [Description("The severity of the reaction")]
        public FhirCode<AllergyIntoleranceSeverity> Severity { get; set; }

    }
}
