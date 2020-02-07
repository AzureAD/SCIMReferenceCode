//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    public sealed class InformationNotification : Notification<string>, IInformationNotification
    {
        public InformationNotification(string payload, bool verbose)
            : base(payload)
        {
            this.Verbose = verbose;
        }

        public InformationNotification(string payload)
            : this(payload, false)
        {
        }

        public InformationNotification(string payload, long identifier)
            : base(payload, identifier)
        {
            this.Verbose = false;
        }

        public InformationNotification(string payload, bool verbose, long identifier)
            : base(payload, identifier)
        {
            this.Verbose = verbose;
        }

        public InformationNotification(
            string payload,
            bool verbose,
            string correlationIdentifier)
            : base(payload, correlationIdentifier)
        {
            this.Verbose = verbose;
        }

        public InformationNotification(
            string payload,
            string correlationIdentifier)
            : this(payload, false, correlationIdentifier)
        {
        }

        public InformationNotification(
            string payload,
            bool verbose,
            string correlationIdentifier,
            long identifier)
            : base(payload, correlationIdentifier, identifier)
        {
            this.Verbose = verbose;
        }

        public InformationNotification(
            string payload,
            string correlationIdentifier,
            long identifier)
            : this(payload, false, correlationIdentifier, identifier)
        {
        }

        public bool Verbose
        {
            get;
            set;
        }
    }
}