//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

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
