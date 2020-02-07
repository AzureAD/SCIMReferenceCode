//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

#pragma warning disable IDE0065 // Misplaced using directive
#pragma warning restore IDE0065 // Misplaced using directive

namespace Microsoft.SCIM.Sample
{
    using System;
    using Microsoft.SCIM;

    public class Startup
    {
        private const string baseAddress = "http://localhost:20000";

        static void Main()
        {
            Service service = new ServiceProvider();
            service.Start(new Uri(Startup.baseAddress));

            Console.ReadKey(true);
        }
    }

    internal class ServiceProvider : Service
    {
        public override IMonitor MonitoringBehavior { get; set; }
        public override IProvider ProviderBehavior { get; set; }

        public ServiceProvider()
        {
            this.MonitoringBehavior = new ConsoleMonitor();
            this.ProviderBehavior = new InMemoryProvider();
        }
    }
}
