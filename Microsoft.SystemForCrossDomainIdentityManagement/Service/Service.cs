//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.IO;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;

    public abstract class Service : IDisposable
    {
        private readonly object thisLock = new object();
        private IDisposable webHost;

        public abstract IMonitor MonitoringBehavior { get; set; }
        public abstract IProvider ProviderBehavior { get; set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.webHost != null)
            {
                lock (this.thisLock)
                {
                    if (this.webHost != null)
                    {
                        this.webHost.Dispose();
                        this.webHost = null;
                    }
                }
            }
        }

        public void Start(Uri baseAddress)
        {
            if (null == baseAddress)
            {
                throw new ArgumentNullException(nameof(baseAddress));
            }

            if (null == this.ProviderBehavior)
            {
                throw new InvalidOperationException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionNotInitializedProviderBehavior);
            }

            if (null == this.MonitoringBehavior)
            {
                throw new InvalidOperationException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionNotInitializedMonitoringBehavior);
            }

            lock (this.thisLock)
            {
                IWebHost host =
                new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureServices(services =>
                {
                    services.AddSingleton(typeof(IProvider), this.ProviderBehavior);
                    services.AddSingleton(typeof(IMonitor), this.MonitoringBehavior);
                })
                .UseStartup<WebApplicationStarter>()
                .Build();

                host.Run();
            }
        }
    }
}
