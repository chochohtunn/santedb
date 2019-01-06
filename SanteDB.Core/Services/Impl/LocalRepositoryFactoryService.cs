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
 * Date: 2018-11-24
 */
using SanteDB.Core.Model;
using System.Diagnostics;

namespace SanteDB.Core.Services.Impl
{
    /// <summary>
    /// Represents a generic resource repository factory
    /// </summary>
    [ServiceProvider("Local Data Repository Factory")]
    public class LocalRepositoryFactoryService : IRepositoryServiceFactory
    {
        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => "Local Data Repository Factory";

        /// <summary>
        /// Create the specified resource service factory
        /// </summary>
        public IRepositoryService<T> CreateRepository<T>() where T : IdentifiedData
        {
            new TraceSource(SanteDBConstants.DataTraceSourceName).TraceEvent(TraceEventType.Warning, 666, "Creating generic repository for {0}. Security may be compromised! Please register an appropriate repository service with the host", typeof(T).FullName);
            return new GenericLocalRepository<T>();
        }

    }
}