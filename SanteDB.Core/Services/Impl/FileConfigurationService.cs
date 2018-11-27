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
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Xml;
using SanteDB.Core.Configuration;

namespace SanteDB.Core.Services.Impl
{
    /// <summary>
    /// Provides a redirected configuration service which reads configuration from a different file
    /// </summary>
    [ServiceProvider("Local Configuration Manager")]
    public class FileConfigurationService : IConfigurationManager
    {
        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => "Local Configuration Manager";

        /// <summary>
        /// Get the configuration
        /// </summary>
        public SanteDBConfiguration Configuration { get; set; }

        // Configuration
        private System.Configuration.Configuration m_configuration = null;

        /// <summary>
        /// Get configuration service
        /// </summary>
        public FileConfigurationService()
        {
            try
            {
                var configFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "santedb.config.xml");
                using (var s = File.OpenRead(configFile))
                    this.Configuration = SanteDBConfiguration.Load(s);
                ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap() { ExeConfigFilename = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "SanteDB.exe.config") }; //Path to your config file
                this.m_configuration = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

            }
            catch { }
        }
        
        /// <summary>
        /// Get the provided section
        /// </summary>
        public T GetSection<T>() where T : IConfigurationSection
        {
            return this.Configuration.GetSection<T>();
        }

        /// <summary>
        /// Get the specified application setting
        /// </summary>
        public string GetAppSetting(string key)
        {
            return this.m_configuration.AppSettings.Settings[key]?.Value;
        }

        /// <summary>
        /// Get connection string
        /// </summary>
        public ConnectionStringInfo GetConnectionString(string key)
        {
            var cs = this.m_configuration.ConnectionStrings.ConnectionStrings[key];
            if(cs != null)
            {
                return new ConnectionStringInfo(cs.ProviderName, cs.ConnectionString);
            }
            return null;
        }

        /// <summary>
        /// Set an application setting
        /// </summary>
        public void SetAppSetting(string key, string value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Reload configuration from disk
        /// </summary>
        public void Reload()
        {
            throw new NotSupportedException();
        }
    }
}
