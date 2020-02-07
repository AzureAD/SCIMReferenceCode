//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;

    public sealed class InformationNotificationFactory : NotificationFactoryBase<IInformationNotification>
    {
        private static readonly Lazy<NotificationFactoryBase<IInformationNotification>> Singleton =
            new Lazy<NotificationFactoryBase<IInformationNotification>>(
                () =>
                    new InformationNotificationFactory());

        private InformationNotificationFactory()
        {
        }

        public static NotificationFactoryBase<IInformationNotification> Instance
        {
            get
            {
                return InformationNotificationFactory.Singleton.Value;
            }
        }

        public override IInformationNotification CreateNotification(
            string payload,
            string correlationIdentifier,
            long? identifier)
        {
            if (string.IsNullOrWhiteSpace(payload))
            {
                throw new ArgumentNullException(nameof(payload));
            }

            IInformationNotification result;
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