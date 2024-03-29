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
using System.Xml.Serialization;

namespace SanteDB.Messaging.FHIR.Resources
{
    /// <summary>
    /// Represents an accreditation
    /// </summary>
    [XmlType("Accreditation", Namespace = "http://hl7.org/fhir")]
    public class Accreditation : FhirElement
    {
        /// <summary>
        /// Gets or sets the identifier for the accreditation
        /// </summary>
        [XmlElement("identifier")]
        public FhirIdentifier Identifier { get; set; }
        /// <summary>
        /// Gets or sets the code (type) of the accreditation
        /// </summary>
        [XmlElement("code")]
        public FhirCodeableConcept Code { get; set; }
        /// <summary>
        /// Gets or sets the issuing organization of the accreditation
        /// </summary>
        [XmlElement("issuer")]
        public Reference<Organization> Issuer { get; set; }
        /// <summary>
        /// Gets or sets the period of the accreditation
        /// </summary>
        [XmlElement("period")]
        public FhirPeriod Period { get; set; }

    }
}
