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
using SanteDB.Core.Services;
using SanteDB.Messaging.FHIR.Resources;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace SanteDB.Messaging.FHIR.Handlers
{

    /// <summary>
    /// Represents a class that can handle a FHIR resource query request
    /// </summary>
    public interface IFhirResourceHandler
    {

        /// <summary>
        /// Gets the type of resource this handler can perform operations on
        /// </summary>
        string ResourceName { get; }

        /// <summary>
        /// Read a specific version of a resource
        /// </summary>
        ResourceBase Read(string id, string versionId);

        /// <summary>
        /// Update a resource
        /// </summary>
        ResourceBase Update(string id, ResourceBase target, TransactionMode mode);

        /// <summary>
        /// Delete a resource
        /// </summary>
        ResourceBase Delete(string id, TransactionMode mode);

        /// <summary>
        /// Create a resource
        /// </summary>
        ResourceBase Create(ResourceBase target, TransactionMode mode);

        /// <summary>
        /// Query a FHIR resource
        /// </summary>
        FhirQueryResult Query(NameValueCollection parameters);

        /// <summary>
        /// Get the history of a specific FHIR object
        /// </summary>
        FhirQueryResult History(string id);

        /// <summary>
        /// Get the definition for this resource
        /// </summary>
        Backbone.ResourceDefinition GetResourceDefinition();

        /// <summary>
        /// Get the structure definition for this profile
        /// </summary>
        StructureDefinition GetStructureDefinition();
    }
}
