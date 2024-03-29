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
using SanteDB.Messaging.FHIR.Resources;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace SanteDB.Messaging.FHIR.Backbone
{

    /// <summary>
    /// Represents additional audit event entity detail
    /// </summary>
    [XmlType("AuditEventEntityDetail", Namespace = "http://hl7.org/fhir")]
    public class AuditEventEntityDetail :BackboneElement
    {
        /// <summary>
        /// Gets or sets the type of data
        /// </summary>
        [XmlElement("type")]
        public FhirString Type { get; set; }

        /// <summary>
        /// Gets or sets the value of the audit event
        /// </summary>
        [XmlElement("value")]
        public FhirBase64Binary Value { get; set; }

    }

    /// <summary>
    /// Represents an entity or data used in the audit event
    /// </summary>
    [XmlType("AuditEventEntity", Namespace = "http://hl7.org/fhir")]
    public class AuditEventEntity : BackboneElement
    {
        /// <summary>
        /// Creates a new audit event entity
        /// </summary>
        public AuditEventEntity()
        {
            this.SecurityLabel = new List<FhirCoding>();
            this.Detail = new List<AuditEventEntityDetail>();
        }

        /// <summary>
        /// Gets or sets the identifier of the audit event object
        /// </summary>
        [XmlElement("identifier")]
        [Description("The identifier of the object")]
        public FhirIdentifier Identifier { get; set; }

        /// <summary>
        /// Gets or sets a reference to the object
        /// </summary>
        [XmlElement("reference")]
        [Description("A reference to the object being accessed")]
        public Reference Reference { get; set; }

        /// <summary>
        /// Gets or sets the type of object involved
        /// </summary>
        [XmlElement("type")]
        [Description("The type of object involved")]
        [FhirElement(RemoteBinding = "http://hl7.org/fhir/ValueSet/audit-entity-type")]
        public FhirCoding Type { get; set; }

        /// <summary>
        /// Gets or sets the role that the object played
        /// </summary>
        [XmlElement("role")]
        [Description("The role that the object played")]
        [FhirElement(RemoteBinding = "http://hl7.org/fhir/ValueSet/object-role")]
        public FhirCoding Role { get; set; }

        /// <summary>
        /// Gets or sets the lifecycle of the object
        /// </summary>
        [XmlElement("lifecycle")]
        [Description("Where in the object's lifecycle the interaction occurred")]
        [FhirElement(RemoteBinding = "http://hl7.org/fhir/ValueSet/object-lifecycle-events")]
        public FhirCoding Lifecycle { get; set; }

        /// <summary>
        /// Gets or sets the security labels on the object
        /// </summary>
        [XmlElement("securityLabel")]
        [Description("Any security or policy labels on the object")]
        [FhirElement(RemoteBinding = "http://hl7.org/fhir/ValueSet/security-labels")]
        public List<FhirCoding> SecurityLabel { get; set; }

        /// <summary>
        /// Gets or sets the name of the object
        /// </summary>
        [XmlElement("name")]
        [Description("A human meaningful name")]
        public FhirString Name { get; set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        [XmlElement("description")]
        [Description("Descriptive text")]
        public FhirString Description { get; set; }

        /// <summary>
        /// Gets or set the query parameters used
        /// </summary>
        [XmlElement("query")]
        [Description("Query parameters used")]
        public FhirBase64Binary Query { get; set; }

        /// <summary>
        /// Gets or sets the audit event entity detail
        /// </summary>
        [XmlElement("detail")]
        [Description("Additional details about the object")]
        public List<AuditEventEntityDetail> Detail { get; set; }


    }
}
