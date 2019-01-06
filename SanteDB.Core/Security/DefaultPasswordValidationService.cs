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
using SanteDB.Core.Configuration;
using SanteDB.Core.Services;
using SanteDB.Core.Services.Impl;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Represents a local regex password validator
    /// </summary>
    [ServiceProvider("Default Password Validator")]
    public class DefaultPasswordValidationService : RegexPasswordValidator
    {
        /// <summary>
        /// Local password validation service
        /// </summary>
        public DefaultPasswordValidationService() : base(ApplicationServiceContext.Current.GetService<IConfigurationManager>().GetSection<SecurityConfigurationSection>().PasswordRegex ?? RegexPasswordValidator.DefaultPasswordPattern)
        {
            
        }
    }
}