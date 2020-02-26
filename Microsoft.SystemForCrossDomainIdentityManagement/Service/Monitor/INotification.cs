// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM
{
    public interface INotification<TPayload>
    {
        long? Identifier { get; }
        string CorrelationIdentifier { get; }
        TPayload Message { get; }
    }
}
