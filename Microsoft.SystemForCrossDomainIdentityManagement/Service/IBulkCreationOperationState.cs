// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System.Collections.Generic;

    public interface IBulkCreationOperationState : IBulkOperationState<Resource>
    {
        IReadOnlyCollection<IBulkUpdateOperationContext> Dependents { get; }
        IReadOnlyCollection<IBulkUpdateOperationContext> Subordinates { get; }

        void AddDependent(IBulkUpdateOperationContext dependent);
        void AddSubordinate(IBulkUpdateOperationContext subordinate);
    }
}
