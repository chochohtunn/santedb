﻿using SanteDB.Core.Persistence;
using SanteDB.Core.Rest;
using SanteDB.Core.Services;
using SanteDB.Core.Services.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Configuration.Features
{
    /// <summary>
    /// Represents a feature for core server implementation
    /// </summary>
    public class CoreServerFeatures : IFeature
    {

        /// <summary>
        /// Services in this configuration
        /// </summary>
        internal static readonly Type[] SERVICE_TYPES =
        {
            typeof(FileConfigurationService),
            typeof(ThreadPoolService),
            typeof(DefaultNetworkInformationService),
            typeof(RestServiceFactory),
            typeof(LocalRepositoryFactoryService),
            typeof(LocalRepositoryService)
        };

        /// <summary>
        /// Gets the name of the feature
        /// </summary>
        public string Name => "SanteDB Server Core";

        /// <summary>
        /// Gets the description
        /// </summary>
        public string Description => "Core features for the SanteDB Server";

        /// <summary>
        /// Get the group
        /// </summary>
        public string Group => "System";

        /// <summary>
        /// Gets the configuration type
        /// </summary>
        public Type ConfigurationType => null;

        /// <summary>
        /// Gets or sets the configuration
        /// </summary>
        public object Configuration { get; set; }

        /// <summary>
        /// Gets the flags for the configuration object
        /// </summary>
        public FeatureFlags Flags => FeatureFlags.NoRemove | FeatureFlags.AutoSetup;

        /// <summary>
        /// Create the installation task
        /// </summary>
        public IEnumerable<IConfigurationTask> CreateInstallTasks()
        {
            return new IConfigurationTask[]
            {
                new InstallCoreServicesTask(this)
            };
        }

        /// <summary>
        /// Create uninstall tasks
        /// </summary>
        public IEnumerable<IConfigurationTask> CreateUninstallTasks()
        {
            return null;
        }

        /// <summary>
        /// Query the status
        /// </summary>
        public FeatureInstallState QueryState(SanteDBConfiguration configuration)
        {
            var nServices = configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Count(s => SERVICE_TYPES.Any(o => o == s.Type));
            if (nServices == SERVICE_TYPES.Length)
                return FeatureInstallState.Installed;
            else if (nServices == 0)
                return FeatureInstallState.NotInstalled;
            else
                return FeatureInstallState.PartiallyInstalled;
        }

        /// <summary>
        /// Represents the core services task
        /// </summary>
        public class InstallCoreServicesTask : IConfigurationTask
        {

            /// <summary>
            /// Create a new install core services task
            /// </summary>
            public InstallCoreServicesTask(CoreServerFeatures feature)
            {
                this.Feature = feature;
            }

            /// <summary>
            /// Get the name of the service task
            /// </summary>
            public string Name => "Install SanteDB Server Services";

            /// <summary>
            /// Gets the description of the task
            /// </summary>
            public string Description => "Installs core services used by SanteDB iCDR Server including configuration, threading, and network information";

            /// <summary>
            /// Gets the feature
            /// </summary>
            public IFeature Feature { get; }

            /// <summary>
            /// Fired when progress of install or execution has changed
            /// </summary>
            public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

            /// <summary>
            /// Execute the installation
            /// </summary>
            public bool Execute(SanteDBConfiguration configuration)
            {
                var appSection = configuration.GetSection<ApplicationServiceContextConfigurationSection>();
                int i = 0;
                foreach (var svc in SERVICE_TYPES.Reverse())
                {
                    this.ProgressChanged?.Invoke(this, new ProgressChangedEventArgs((float)i++ / SERVICE_TYPES.Length, $"Installing {svc.Name}"));
                    appSection.ServiceProviders.RemoveAll(o => o.Type == svc);
                    appSection.ServiceProviders.Insert(0, new TypeReferenceConfiguration(svc));
                }
                return true;
            }

            /// <summary>
            /// Rollback any changes
            /// </summary>
            public bool Rollback(SanteDBConfiguration configuration)
            {
                var appSection = configuration.GetSection<ApplicationServiceContextConfigurationSection>();
                foreach (var svc in SERVICE_TYPES.Reverse())
                    appSection.ServiceProviders.RemoveAll(o => o.Type == svc);
                return true;
            }

            /// <summary>
            /// Verify that this task can run
            /// </summary>
            public bool VerifyState(SanteDBConfiguration configuration)
            {
                return true;
            }
        }
    }
}