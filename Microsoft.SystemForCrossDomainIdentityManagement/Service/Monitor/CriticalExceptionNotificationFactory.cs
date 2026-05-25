// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;

    public sealed class CriticalExceptionNotificationFactory : NotificationFactory<Exception, IExceptionNotification>
    {
        private static readonly Lazy<NotificationFactory<Exception, IExceptionNotification>> Singleton =
            new Lazy<NotificationFactory<Exception, IExceptionNotification>>(
                () =>
                    new CriticalExceptionNotificationFactory());

        private CriticalExceptionNotificationFactory()
        {
        }

        public static NotificationFactory<Exception, IExceptionNotification> Instance
        {
            get
            {
                return CriticalExceptionNotificationFactory.Singleton.Value;
            }
        }

        public override IExceptionNotification CreateNotification(
            Exception payload,
            string correlationIdentifier,
            long? identifier)
        {
            if (null == payload)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            IExceptionNotification result;
            if (string.IsNullOrWhiteSpace(correlationIdentifier))
            {
                if (!identifier.HasValue)
                {
                    result = new ExceptionNotification(payload, true);
                }
                else
                {
                    result = new ExceptionNotification(payload, true, identifier.Value);
                }
            }
            else
            {
                if (!identifier.HasValue)
                {
                    result = new ExceptionNotification(payload, true, correlationIdentifier);
                }
                else
                {
                    result = new ExceptionNotification(payload, true, correlationIdentifier, identifier.Value);
                }
            }
            return result;
        }
    }
}
