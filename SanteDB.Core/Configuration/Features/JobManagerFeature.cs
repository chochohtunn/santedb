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
 * Date: 2019-3-2
 */
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Configuration.Features
{
    /// <summary>
    /// Represents a feature for the job manager
    /// </summary>
    public class JobManagerFeature : GenericServiceFeature<DefaultJobManagerService>
    {

        /// <summary>
        /// Job manager feature ctor
        /// </summary>
        public JobManagerFeature()
        {
            this.Configuration = new JobConfigurationSection()
            {
                Jobs = new List<JobItemConfiguration>()
            };
        }

        /// <summary>
        /// Setup the job manager
        /// </summary>
        public override FeatureFlags Flags => FeatureFlags.AutoSetup; 

        /// <summary>
        /// Gets the description
        /// </summary>
        public override string Description => "Allows SanteDB to run scheduled or ad-hoc 'jobs' (such as compression, warehousing, backup)";

    }
}
