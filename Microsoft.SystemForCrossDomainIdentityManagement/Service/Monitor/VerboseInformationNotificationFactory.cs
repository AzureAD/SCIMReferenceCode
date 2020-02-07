//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;

    public sealed class VerboseInformationNotificationFactory : NotificationFactoryBase<IInformationNotification>
    {
        private static readonly Lazy<NotificationFactoryBase<IInformationNotification>> Singleton =
            new Lazy<NotificationFactoryBase<IInformationNotification>>(
                () =>
                    new VerboseInformationNotificationFactory());

        private VerboseInformationNotificationFactory()
        {
        }

        public static NotificationFactoryBase<IInformationNotification> Instance
        {
            get
            {
                return VerboseInformationNotificationFactory.Singleton.Value;
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
                    result = new InformationNotification(payload, true);
                }
                else
                {
                    result = new InformationNotification(payload, true, identifier.Value);
                }
            }
            else
            {
                if (!identifier.HasValue)
                {
                    result = new InformationNotification(payload, true, correlationIdentifier);
                }
                else
                {
                    result = new InformationNotification(payload, true, correlationIdentifier, identifier.Value);
                }
            }
            return result;
        }
    }
}