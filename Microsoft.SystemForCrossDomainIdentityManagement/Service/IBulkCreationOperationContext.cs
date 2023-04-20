// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    public interface IBulkCreationOperationContext : IBulkOperationContext<Resource>, IBulkCreationOperationState
    {
        IBulkOperationState<Resource> PendingState { get; }
    }
}
