//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;

    public sealed class SevereWarningNotificationFactory : NotificationFactory<Exception, Notification<Exception>>
    {
        private static readonly Lazy<SevereWarningNotificationFactory> Singleton =
            new Lazy<SevereWarningNotificationFactory>(
                () =>
                    new SevereWarningNotificationFactory());

        private SevereWarningNotificationFactory()
        {
        }

        public static SevereWarningNotificationFactory Instance
        {
            get
            {
                return SevereWarningNotificationFactory.Singleton.Value;
            }
        }

        public override Notification<Exception> CreateNotification(
            Exception payload,
            string correlationIdentifier,
            long? identifier)
        {
            if (null == payload)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            Notification<Exception> result;
            if (string.IsNullOrWhiteSpace(correlationIdentifier))
            {
                if (!identifier.HasValue)
                {
                    result = new ExceptionNotification(payload);
                }
                else
                {
                    result = new ExceptionNotification(payload, identifier.Value);
                }
            }
            else
            {
                if (!identifier.HasValue)
                {
                    result = new ExceptionNotification(payload, correlationIdentifier);
                }
                else
                {
                    result = new ExceptionNotification(payload, correlationIdentifier, identifier.Value);
                }
            }
            return result;
        }
    }
}