// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    public interface INotification<TPayload>
    {
        long? Identifier { get; }
        string CorrelationIdentifier { get; }
        TPayload Message { get; }
    }
}
