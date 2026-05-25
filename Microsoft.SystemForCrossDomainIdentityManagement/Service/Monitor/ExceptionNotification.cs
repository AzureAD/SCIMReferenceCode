// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;

    public sealed class ExceptionNotification : Notification<Exception>, IExceptionNotification
    {
        public ExceptionNotification(Exception payload, bool critical)
            : base(payload)
        {
            this.Critical = critical;
        }

        public ExceptionNotification(Exception payload)
            : this(payload, false)
        {
        }

        public ExceptionNotification(Exception payload, long identifier)
            : base(payload, identifier)
        {
            this.Critical = false;
        }

        public ExceptionNotification(Exception payload, bool critical, long identifier)
            : base(payload, identifier)
        {
            this.Critical = critical;
        }

        public ExceptionNotification(Exception payload, bool critical, string correlationIdentifier)
            : base(payload, correlationIdentifier)
        {
            this.Critical = critical;
        }

        public ExceptionNotification(Exception payload, string correlationIdentifier)
            : this(payload, false, correlationIdentifier)
        {
        }

        public ExceptionNotification(Exception payload, bool critical, string correlationIdentifier, long identifier)
            : base(payload, correlationIdentifier, identifier)
        {
            this.Critical = critical;
        }

        public ExceptionNotification(Exception payload, string correlationIdentifier, long identifier)
            : this(payload, false, correlationIdentifier, identifier)
        {
        }

        public bool Critical
        {
            get;
            set;
        }
    }
}
