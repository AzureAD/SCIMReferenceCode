// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Http;

    public interface IRequest
    {
        Uri BaseResourceIdentifier { get; }
        string CorrelationIdentifier { get; }
        IReadOnlyCollection<IExtension> Extensions { get; }
        HttpContext HttpContext { get; }
    }

    public interface IRequest<TPayload> : IRequest where TPayload : class
    {
        TPayload Payload { get; }
    }
}
