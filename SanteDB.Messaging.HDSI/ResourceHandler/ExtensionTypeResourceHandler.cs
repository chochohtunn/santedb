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
 * User: fyfej
 * Date: 2017-9-1
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Model.DataTypes;
using MARC.HI.EHRS.SVC.Core;
using MARC.HI.EHRS.SVC.Core.Services;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Attribute;
using SanteDB.Core.Services;
using SanteDB.Core.Interop;
using SanteDB.Messaging.Common;

namespace SanteDB.Messaging.HDSI.ResourceHandler
{
    /// <summary>
    /// Represents a handler for extension types
    /// </summary>
    public class ExtensionTypeResourceHandler : IResourceHandler
    {

        /// <summary>
        /// Get capabilities of this handler
        /// </summary>
        public ResourceCapability Capabilities
        {
            get
            {
                return ResourceCapability.Get | ResourceCapability.Search;
            }
        }

        /// <summary>
        /// Resource name
        /// </summary>
        public string ResourceName
        {
            get
            {
                return "ExtensionType";
            }
        }

        /// <summary>
        /// Gets the scope
        /// </summary>
        public Type Scope => typeof(Wcf.IHdsiServiceContract);

        /// <summary>
        /// Gets the type of the handler
        /// </summary>
        public Type Type
        {
            get
            {
                return typeof(ExtensionType);
            }
        }

        /// <summary>
        /// Readonly
        /// </summary>
        public Object Create(Object data, bool updateIfExists)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Get the extension
        /// </summary>
        [PolicyPermission(System.Security.Permissions.SecurityAction.Demand, PolicyId = PermissionPolicyIdentifiers.ReadMetadata)]
        public Object Get(object id, object versionId)
        {
            var repository = ApplicationContext.Current.GetService<IDataPersistenceService<ExtensionType>>();
            return repository?.Get<Guid>(new MARC.HI.EHRS.SVC.Core.Data.Identifier<Guid>((Guid)id, (Guid)versionId), AuthenticationContext.Current.Principal, false);
        }

        /// <summary>
        /// Read only
        /// </summary>
        public Object Obsolete(object  key)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Query the specified types
        /// </summary>
        [PolicyPermission(System.Security.Permissions.SecurityAction.Demand, PolicyId = PermissionPolicyIdentifiers.ReadMetadata)]
        public IEnumerable<Object> Query(NameValueCollection queryParameters)
        {
            int tr = 0;
            return this.Query(queryParameters, 0, 100, out tr);
        }

        /// <summary>
        /// Query with offset and count
        /// </summary>
        [PolicyPermission(System.Security.Permissions.SecurityAction.Demand, PolicyId = PermissionPolicyIdentifiers.ReadMetadata)]
        public IEnumerable<Object> Query(NameValueCollection queryParameters, int offset, int count, out int totalCount)
        {
            var repository = ApplicationContext.Current.GetService<IDataPersistenceService<ExtensionType>>();
            var filter = QueryExpressionParser.BuildLinqExpression<ExtensionType>(queryParameters);
            List<String> queryId = null;
            if (repository is IStoredQueryDataPersistenceService<ExtensionType> && queryParameters.TryGetValue("_queryId", out queryId))
                return (repository as IStoredQueryDataPersistenceService<ExtensionType>).Query(filter, Guid.Parse(queryId[0]), offset, count, AuthenticationContext.Current.Principal, out totalCount);
            else
                return repository.Query(filter, offset, count, AuthenticationContext.Current.Principal, out totalCount);
        }

        /// <summary>
        /// Readonly
        /// </summary>
        public Object Update(Object  data)
        {
            throw new NotSupportedException();
        }
    }
}
