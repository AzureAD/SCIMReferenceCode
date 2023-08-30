// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System.Collections.Generic;

    public interface IBulkUpdateOperationState : IBulkOperationState<IPatch>
    {
        IReadOnlyCollection<IBulkCreationOperationContext> Dependencies { get; }
        IBulkCreationOperationContext Parent { get; }

        void AddDependency(IBulkCreationOperationContext dependency);
    }
}
