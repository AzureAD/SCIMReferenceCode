//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;

    public abstract class SystemForCrossDomainIdentityManagementRequest<TPayload> : IRequest<TPayload>
        where TPayload : class
    {
        protected SystemForCrossDomainIdentityManagementRequest(
            HttpRequestMessage request,
            TPayload payload,
            string correlationIdentifier,
            IReadOnlyCollection<IExtension> extensions)
        {
            if (null == request)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(correlationIdentifier))
            {
                throw new ArgumentNullException(nameof(extensions));
            }

            this.BaseResourceIdentifier = request.GetBaseResourceIdentifier();
            this.Request = request;
            this.Payload = payload ?? throw new ArgumentNullException(nameof(payload));
            this.CorrelationIdentifier = correlationIdentifier;
            this.Extensions = extensions;
        }

        public Uri BaseResourceIdentifier
        {
            get;
            private set;
        }

        public string CorrelationIdentifier
        {
            get;
            private set;
        }

        public IReadOnlyCollection<IExtension> Extensions
        {
            get;
            private set;
        }

        public TPayload Payload
        {
            get;
            private set;
        }

        public HttpRequestMessage Request
        {
            get;
            private set;
        }
    }
}