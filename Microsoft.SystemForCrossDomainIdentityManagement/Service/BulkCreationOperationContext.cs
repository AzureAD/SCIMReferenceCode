//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;

    internal sealed class BulkCreationOperationContext : BulkOperationContextBase<Resource>, IBulkCreationOperationContext
    {
        private readonly IBulkCreationOperationState receivedState;

        public BulkCreationOperationContext(
            IRequest<BulkRequest2> request,
            BulkRequestOperation operation)
        {
            if (null == request)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (null == operation)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            this.receivedState = new BulkCreationOperationState(request, operation, this);
            this.Initialize(this.receivedState);
            this.PendingState = new BulkOperationState<Resource>(request, operation, this);
        }

        public IReadOnlyCollection<IBulkUpdateOperationContext> Dependents => this.receivedState.Dependents;
        
        public IBulkOperationState<Resource> PendingState
        {
            get;
            private set;
        }

        public IReadOnlyCollection<IBulkUpdateOperationContext> Subordinates => this.receivedState.Subordinates;
        
        public void AddDependent(IBulkUpdateOperationContext dependent)
        {
            if (null == dependent)
            {
                throw new ArgumentNullException(nameof(dependent));
            }

            this.receivedState.AddDependent(dependent);
        }

        public void AddSubordinate(IBulkUpdateOperationContext subordinate)
        {
            if (null == subordinate)
            {
                throw new ArgumentNullException(nameof(subordinate));
            }

            this.receivedState.AddSubordinate(subordinate);
        }
    }
}
