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
using SanteDB.Core.Model;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Interfaces;
using System;

namespace SanteDB.Core.Services.Impl
{
    /// <summary>
    /// Generic nullifiable local repository
    /// </summary>
    public class GenericLocalNullifiedRepository<TModel> : GenericLocalRepository<TModel>, INullifyRepositoryService<TModel>
        where TModel : IdentifiedData, IHasState
    {
        /// <summary>
        /// Nullify the specified object
        /// </summary>
        public virtual TModel Nullify(Guid id)
        {
            var target = base.Get(id); ;
            target.StatusConceptKey = StatusKeys.Nullified;
            return base.Save(target);
        }
    }
}