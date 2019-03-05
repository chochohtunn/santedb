﻿/*
 * Copyright 2015-2019 Mohawk College of Applied Arts and Technology
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
 * User: JustinFyfe
 * Date: 2019-1-22
 */
using SanteDB.Core.Model;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Security;

namespace SanteDB.Core.Services.Impl
{
    /// <summary>
    /// Represents generic local clinical data repository
    /// </summary>
    public class GenericLocalClinicalDataRepository<TModel> : GenericLocalNullifiedRepository<TModel> where TModel : IdentifiedData, IHasState
    {
        protected override string QueryPolicy => PermissionPolicyIdentifiers.QueryClinicalData;
        protected override string ReadPolicy => PermissionPolicyIdentifiers.ReadClinicalData;
        protected override string WritePolicy => PermissionPolicyIdentifiers.WriteClinicalData;
        protected override string DeletePolicy => PermissionPolicyIdentifiers.DeleteClinicalData;
        protected override string AlterPolicy => PermissionPolicyIdentifiers.WriteClinicalData;
    }
}