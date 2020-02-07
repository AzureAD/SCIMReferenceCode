//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    public interface INotification<TPayload>
    {
        long? Identifier { get; }
        string CorrelationIdentifier { get; }
        TPayload Message { get; }
    }
}