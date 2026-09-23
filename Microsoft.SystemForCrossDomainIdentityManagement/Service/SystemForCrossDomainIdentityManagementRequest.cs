// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Http;

    public class SystemForCrossDomainIdentityManagementRequest<TPayload> : IRequest<TPayload>
        where TPayload : class
    {
        public SystemForCrossDomainIdentityManagementRequest(
            HttpContext context,
            TPayload payload,
            string correlationIdentifier,
            IReadOnlyCollection<IExtension> extensions)
        {
            if (null == context)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (string.IsNullOrWhiteSpace(correlationIdentifier))
            {
                throw new ArgumentNullException(nameof(extensions));
            }

            this.BaseResourceIdentifier = context.GetBaseResourceIdentifier();
            this.HttpContext = context;
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

        public HttpContext HttpContext
        {
            get;
            private set;
        }
    }
}
