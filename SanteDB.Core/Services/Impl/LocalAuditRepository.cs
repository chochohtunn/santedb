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
 * Date: 2018-6-22
 */
using SanteDB.Core.Auditing;
using SanteDB.Core.Interfaces;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Attribute;
using SanteDB.Core.Security.Services;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SanteDB.Core.Services.Impl
{
    /// <summary>
    /// Represents an audit repository which stores and queries audit data.
    /// </summary>
    [ServiceProvider("Default Audit Repository")]
    public class LocalAuditRepository : IAuditRepositoryService, IAuditEventSource
    {

        /// <summary>
        /// Gets the service name
        /// </summary>
        public String ServiceName => "Default Audit Repository";

        /// <summary>
        /// Fired when data has been created
        /// </summary>
        public event EventHandler<AuditDataEventArgs> DataCreated;
        /// <summary>
        /// Fired when data has been disclosed
        /// </summary>
        public event EventHandler<AuditDataDisclosureEventArgs> DataDisclosed;
        /// <summary>
        /// Fired when data has been obsoleted
        /// </summary>
        public event EventHandler<AuditDataEventArgs> DataObsoleted;
        /// <summary>
        /// Fired when data has been updated
        /// </summary>
        public event EventHandler<AuditDataEventArgs> DataUpdated;


        /// <summary>
        /// Find the specified data
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public IEnumerable<AuditData> Find(Expression<Func<AuditData, bool>> query)
        {
            var tr = 0;
            return this.Find(query, 0, null, out tr);
        }

        /// <summary>
        /// Find with query controls
        /// </summary>
        [PolicyPermission(System.Security.Permissions.SecurityAction.Demand, PolicyId = PermissionPolicyIdentifiers.AccessAuditLog)]
        public IEnumerable<AuditData> Find(Expression<Func<AuditData, bool>> query, int offset, int? count, out int totalResults)
        {
            var service = ApplicationServiceContext.Current.GetService<IDataPersistenceService<AuditData>>();
            if (service == null)
                throw new InvalidOperationException("Cannot find the data persistence service for audits");
            var results = service.Query(query, offset, count, out totalResults, AuthenticationContext.Current.Principal);
            this.DataDisclosed?.Invoke(this, new AuditDataDisclosureEventArgs(query.ToString(), results));
            return results;
        }

        /// <summary>
        /// Gets the specified object
        /// </summary>
        public AuditData Get(Guid key)
        {
            return this.Get(key, Guid.Empty);
        }

        /// <summary>
        /// Gets the specified correlation key
        /// </summary>
        public AuditData Get(object correlationKey)
        {
            return this.Get((Guid)correlationKey, Guid.Empty);
        }

        /// <summary>
        /// Get the specified audit by key
        /// </summary>
        [PolicyPermission(System.Security.Permissions.SecurityAction.Demand, PolicyId = PermissionPolicyIdentifiers.AccessAuditLog)]
        public AuditData Get(Guid key, Guid versionKey)
        {
            var service = ApplicationServiceContext.Current.GetService<IDataPersistenceService<AuditData>>();
            if (service == null)
                throw new InvalidOperationException("Cannot find the data persistence service for audits");
            var result = service.Get(key, versionKey, false, AuthenticationContext.Current.Principal);
            this.DataDisclosed?.Invoke(this, new AuditDataDisclosureEventArgs(key.ToString(), new List<Object>() { result }));
            return result;
        }

        /// <summary>
        /// Insert the specified data
        /// </summary>
        public AuditData Insert(AuditData audit)
        {
            var service = ApplicationServiceContext.Current.GetService<IDataPersistenceService<AuditData>>();
            if (service == null)
                throw new InvalidOperationException("Cannot find the data persistence service for audits");
            var result = service.Insert(audit, TransactionMode.Commit, AuthenticationContext.Current.Principal);
            this.DataCreated?.Invoke(this, new AuditDataEventArgs(audit));
            return result;
        }

        /// <summary>
        /// Obsolete the specified data
        /// </summary>
        [PolicyPermission(System.Security.Permissions.SecurityAction.Demand, PolicyId = PermissionPolicyIdentifiers.AccessAuditLog)]
        public AuditData Obsolete(Guid key)
        {
            var service = ApplicationServiceContext.Current.GetService<IDataPersistenceService<AuditData>>();
            if (service == null)
                throw new InvalidOperationException("Cannot find the data persistence service for audits");
            var result = service.Obsolete(new AuditData() { Key = key }, TransactionMode.Commit, AuthenticationContext.Current.Principal);
            this?.DataObsoleted?.Invoke(this, new AuditDataEventArgs(key));
            return result;
        }

        /// <summary>
        /// Save (create or update) the specified object
        /// </summary>
        [PolicyPermission(System.Security.Permissions.SecurityAction.Demand, PolicyId = PermissionPolicyIdentifiers.AccessAuditLog)]
        public AuditData Save(AuditData data)
        {
            var service = ApplicationServiceContext.Current.GetService<IDataPersistenceService<AuditData>>();
            if (service == null)
                throw new InvalidOperationException("Cannot find the data persistence service for audits");

            var existing = service.Get(data.Key.Value, Guid.Empty, false, AuthenticationContext.Current.Principal);
            if (existing == null)
            {
                data = service.Update(data, TransactionMode.Commit, AuthenticationContext.Current.Principal);
                this.DataUpdated?.Invoke(this, new AuditDataEventArgs(data));
            }
            else
            {
                data = service.Insert(data, TransactionMode.Commit, AuthenticationContext.Current.Principal);
                this.DataCreated?.Invoke(this, new AuditDataEventArgs(data));
            }
            return data;
        }
    }
}
