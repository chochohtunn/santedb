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
using SanteDB.OrmLite;
using System;
using System.Collections;

namespace SanteDB.Persistence.Data.ADO.Services
{
    /// <summary>
    /// Represents an ADO based IDataPersistenceServie
    /// </summary>
    public interface IAdoPersistenceService : IDataPersistenceService
    {
        /// <summary>
        /// Inserts the specified object
        /// </summary>
        Object Insert(DataContext context, Object data);

        /// <summary>
        /// Updates the specified data
        /// </summary>
        Object Update(DataContext context, Object data);

        /// <summary>
        /// Obsoletes the specified data
        /// </summary>
        Object Obsolete(DataContext context, Object data);

        /// <summary>
        /// Gets the specified data
        /// </summary>
        Object Get(DataContext context, Guid id);

        /// <summary>
        /// Map to model instance
        /// </summary>
        Object ToModelInstance(object domainInstance, DataContext context);
        
    }

    /// <summary>
    /// ADO associative persistence service
    /// </summary>
    public interface IAdoAssociativePersistenceService : IAdoPersistenceService
    {
        /// <summary>
        /// Get the set objects from the source
        /// </summary>
        IEnumerable GetFromSource(DataContext context, Guid id, decimal? versionSequenceId);
    }
}