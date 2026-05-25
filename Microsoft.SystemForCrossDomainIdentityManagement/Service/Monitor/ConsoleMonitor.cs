// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;
    using System.Globalization;
    using System.Threading;

    public sealed class ConsoleMonitor : IMonitor
    {
        private const string PrefixTemplate = "{0}{1}<{2}>";

        private static readonly Lazy<string> CorrelationIdentifierDefault =
            new Lazy<string>(
                () =>
                    Guid.Empty.ToString());

        private static string ComposePrefix<TPayload>(INotification<TPayload> notification)
        {
            if (null == notification)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            string effectiveCorrelationIdentifier =
                string.IsNullOrWhiteSpace(notification.CorrelationIdentifier) ?
                    ConsoleMonitor.CorrelationIdentifierDefault.Value : notification.CorrelationIdentifier;
            string effectiveMessageIdentifier =
                notification.Identifier.HasValue ?
                    string.Empty : string.Format(CultureInfo.InvariantCulture, SystemForCrossDomainIdentityManagementServiceResources.MonitorCorrelationIdentifierPrefixTemplate, notification.Identifier);
            string result =
                string.Format(
                    CultureInfo.InvariantCulture,
                    ConsoleMonitor.PrefixTemplate,
                    effectiveMessageIdentifier,
                    effectiveCorrelationIdentifier,
                    Thread.CurrentThread.ManagedThreadId);
            return result;
        }

        public void Inform(IInformationNotification notification)
        {
            if (null == notification)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            string prefix = ConsoleMonitor.ComposePrefix<string>(notification);
            Console.WriteLine(
                SystemForCrossDomainIdentityManagementServiceResources.MonitorOutputInformationTemplate,
                prefix,
                notification.Message,
                notification.Verbose);
        }

        public void Report(IExceptionNotification notification)
        {
            if (null == notification)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            string prefix = ConsoleMonitor.ComposePrefix<Exception>(notification);
            Console.WriteLine(
                SystemForCrossDomainIdentityManagementServiceResources.MonitorOutputExceptionTemplate,
                prefix,
                notification.Message,
                notification.Critical);
        }

        public void Warn(Notification<Exception> notification)
        {
            if (null == notification)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            string prefix = ConsoleMonitor.ComposePrefix<Exception>(notification);
            Console.WriteLine(
                SystemForCrossDomainIdentityManagementServiceResources.MonitorOutputTemplate,
                prefix,
                notification.Message);
        }

        public void Warn(Notification<string> notification)
        {
            if (null == notification)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            string prefix = ConsoleMonitor.ComposePrefix<string>(notification);
            Console.WriteLine(
                SystemForCrossDomainIdentityManagementServiceResources.MonitorOutputTemplate,
                prefix,
                notification.Message);
        }
    }
}
