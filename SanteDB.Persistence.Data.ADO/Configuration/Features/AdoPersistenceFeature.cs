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
 * Date: 2019-2-28
 */
using SanteDB.Core.Configuration;
using SanteDB.Core.Configuration.Features;
using SanteDB.OrmLite.Configuration;
using SanteDB.OrmLite.Migration;
using SanteDB.Persistence.Data.ADO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Persistence.Data.ADO.Configuration.Features
{
    /// <summary>
    /// Represents an ADO persistence service
    /// </summary>
    public class AdoPersistenceFeature : GenericServiceFeature<AdoPersistenceService>
    {

        /// <summary>
        /// Set the default configuration
        /// </summary>
        public AdoPersistenceFeature() : base()
        {
            this.Configuration = new AdoPersistenceConfigurationSection()
            {
                AutoInsertChildren = true,
                AutoUpdateExisting = true,
                PrepareStatements = true
            };
        }

        /// <summary>
        /// Create the installation tasks
        /// </summary>
        public override IEnumerable<IConfigurationTask> CreateInstallTasks()
        {
            // Add installation tasks
            List<IConfigurationTask> retVal = new List<IConfigurationTask>(base.CreateInstallTasks());
            var conf = this.Configuration as OrmConfigurationBase;

            foreach (var feature in SqlFeatureUtil.GetFeatures(conf.Provider.Invariant).OfType<SqlFeature>().Where(o => o.Scope == "SanteDB.Persistence.Data.ADO").OrderBy(o=>o.Id))
                retVal.Add(new SqlMigrationTask(this, feature));
            return retVal;
        }
    }
}
