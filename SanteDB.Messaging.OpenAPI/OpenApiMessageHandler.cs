﻿using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSrvr;
using SanteDB.Core.Interop;
using SanteDB.Core;

namespace SanteDB.Messaging.OpenAPI
{
    /// <summary>
    /// Represents the daemon service that starts/stops the OpenAPI information file
    /// </summary>
    public class OpenApiMessageHandler : IDaemonService, IApiEndpointProvider
    {

        /// <summary>
        /// Represents a rest service
        /// </summary>
        private RestService m_webHost;

        /// <summary>
        /// Get whether the daemon service is running
        /// </summary>
        public bool IsRunning => this.m_webHost?.IsRunning == true;

        /// <summary>
        /// Get the name of this service
        /// </summary>
        public string ServiceName => "OpenAPI Metadata Service";

        /// <summary>
        /// Gets the API type
        /// </summary>
        public ServiceEndpointType ApiType => ServiceEndpointType.Metadata;

        /// <summary>
        /// URL of the service
        /// </summary>
        public string[] Url => this.m_webHost.Endpoints.OfType<ServiceEndpoint>().Select(o => o.Description.ListenUri.ToString()).ToArray();

        /// <summary>
        /// Get capabilities of this endpoint
        /// </summary>
        public ServiceEndpointCapabilities Capabilities => (ServiceEndpointCapabilities)ApplicationServiceContext.Current.GetService<IRestServiceFactory>().GetServiceCapabilities(this.m_webHost);

        /// <summary>
        /// Fired when the service is starting
        /// </summary>
        public event EventHandler Starting;
        /// <summary>
        /// Fired when the service has started
        /// </summary>
        public event EventHandler Started;
        /// <summary>
        /// Fired when the service is stopping
        /// </summary>
        public event EventHandler Stopping;
        /// <summary>
        /// Fired when the service has stopped
        /// </summary>
        public event EventHandler Stopped;

        /// <summary>
        /// Starts the service and binds the service endpoints
        /// </summary>
        public bool Start()
        {
            this.Starting?.Invoke(this, EventArgs.Empty);

            this.Started?.Invoke(this, EventArgs.Empty);
            return true;
        }

        /// <summary>
        /// Stop the service
        /// </summary>
        public bool Stop()
        {
            this.Stopping?.Invoke(this, EventArgs.Empty);

            this.m_webHost.Stop();

            this.Stopped?.Invoke(this, EventArgs.Empty);
            return true;
        }
    }
}