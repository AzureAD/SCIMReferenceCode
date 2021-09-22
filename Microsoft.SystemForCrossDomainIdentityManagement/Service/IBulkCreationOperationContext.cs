//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    public interface IBulkCreationOperationContext : IBulkOperationContext<Resource>, IBulkCreationOperationState
    {
        IBulkOperationState<Resource> PendingState { get; }
    }
}
