//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;

    internal sealed class BulkUpdateOperationContext : BulkOperationContextBase<IPatch>, IBulkUpdateOperationContext
    {
        private readonly IBulkUpdateOperationState receivedState;

        public BulkUpdateOperationContext(
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

            this.receivedState = new BulkUpdateOperationState(request, operation, this);
            this.Initialize(this.receivedState);
        }

        public BulkUpdateOperationContext(
            IRequest<BulkRequest2> request,
            BulkRequestOperation operation,
            IBulkCreationOperationContext parent)
        {
            if (null == request)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (null == operation)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            this.receivedState = new BulkUpdateOperationState(request, operation, this, parent);
            this.Initialize(this.receivedState);
        }

        public IReadOnlyCollection<IBulkCreationOperationContext> Dependencies => this.receivedState.Dependencies;
        
        public IBulkCreationOperationContext Parent => this.receivedState.Parent;

        public void AddDependency(IBulkCreationOperationContext dependency)
        {
            if (null == dependency)
            {
                throw new ArgumentNullException(nameof(dependency));
            }

            this.receivedState.AddDependency(dependency);
        }
    }
}
