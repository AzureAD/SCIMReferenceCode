//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Globalization;

    public abstract class NotificationFactoryBase<TNotification> : NotificationFactory<string, TNotification>
    {
        public abstract override TNotification CreateNotification(
            string payload,
            string correlationIdentifier,
            long? identifier);

        public TNotification FormatNotification(
            string template,
            string correlationIdentifier,
            long? identifier,
            params object[] arguments)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                throw new ArgumentNullException(nameof(template));
            }

            string payload = string.Format(CultureInfo.InvariantCulture, template, arguments);
            TNotification result = this.CreateNotification(payload, correlationIdentifier, identifier);
            return result;
        }

        public TNotification FormatNotification(
            string template,
            Guid correlationIdentifier,
            long? identifier,
            params object[] arguments)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                throw new ArgumentNullException(nameof(template));
            }

            string correlationIdentifierValue = correlationIdentifier.ToString();
            TNotification result = this.FormatNotification(template, correlationIdentifierValue, identifier, arguments);
            return result;
        }
    }
}