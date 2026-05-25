// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;

    public sealed class WarningNotificationFactory : NotificationFactoryBase<Notification<string>>
    {
        private static readonly Lazy<WarningNotificationFactory> Singleton =
            new Lazy<WarningNotificationFactory>(
                () =>
                    new WarningNotificationFactory());

        private WarningNotificationFactory()
        {
        }

        public static WarningNotificationFactory Instance
        {
            get
            {
                return WarningNotificationFactory.Singleton.Value;
            }
        }

        public override Notification<string> CreateNotification(
            string payload,
            string correlationIdentifier,
            long? identifier)
        {
            if (string.IsNullOrWhiteSpace(payload))
            {
                throw new ArgumentNullException(nameof(payload));
            }

            Notification<string> result;
            if (string.IsNullOrWhiteSpace(correlationIdentifier))
            {
                if (!identifier.HasValue)
                {
                    result = new InformationNotification(payload);
                }
                else
                {
                    result = new InformationNotification(payload, identifier.Value);
                }
            }
            else
            {
                if (!identifier.HasValue)
                {
                    result = new InformationNotification(payload, correlationIdentifier);
                }
                else
                {
                    result = new InformationNotification(payload, correlationIdentifier, identifier.Value);
                }
            }
            return result;
        }
    }
}
