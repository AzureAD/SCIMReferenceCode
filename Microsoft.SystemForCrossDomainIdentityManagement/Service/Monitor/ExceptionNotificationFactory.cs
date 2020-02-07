//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;

    public sealed class ExceptionNotificationFactory : NotificationFactory<Exception, IExceptionNotification>
    {
        private static readonly Lazy<NotificationFactory<Exception, IExceptionNotification>> Singleton =
            new Lazy<NotificationFactory<Exception, IExceptionNotification>>(
                () =>
                    new ExceptionNotificationFactory());

        private ExceptionNotificationFactory()
        {
        }

        public static NotificationFactory<Exception, IExceptionNotification> Instance
        {
            get
            {
                return ExceptionNotificationFactory.Singleton.Value;
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