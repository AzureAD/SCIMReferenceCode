// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;

    public interface IRequest
    {
        Uri BaseResourceIdentifier { get; }
        string CorrelationIdentifier { get; }
        IReadOnlyCollection<IExtension> Extensions { get; }
        HttpRequestMessage Request { get; }
    }

    public interface IRequest<TPayload> : IRequest where TPayload : class
    {
        TPayload Payload { get; }
    }
}
