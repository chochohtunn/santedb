﻿/*
 * Copyright 2015-2018 Mohawk College of Applied Arts and Technology
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
 * User: justin
 * Date: 2018-11-23
 */
using SanteDB.Messaging.FHIR.Attributes;
using SanteDB.Messaging.FHIR.DataTypes;
using System.ComponentModel;
using System.Xml.Serialization;

namespace SanteDB.Messaging.FHIR.Backbone
{
    /// <summary>
    /// Represents packaging of a particular medication
    /// </summary>
    [XmlType("MedicationPackage", Namespace = "http://hl7.org/fhir")]
    public class MedicationPackage : BackboneElement
    {

        /// <summary>
        /// Gets or sets information about the container
        /// </summary>
        [XmlElement("container")]
        [Description("Type of container")]
        [FhirElement(RemoteBinding = "http://hl7.org/fhir/ValueSet/medication-package-form")]
        public FhirCodeableConcept Container { get; set; }

        /// <summary>
        /// Gets or sets details about the batch
        /// </summary>
        [XmlElement("batch")]
        [Description("Represents the batch of a particular package")]
        public MedicationBatch Batch { get; set; }
    }
}