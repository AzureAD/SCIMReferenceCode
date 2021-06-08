//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;

    internal sealed class BulkDeletionOperationContext : BulkOperationContextBase<IResourceIdentifier>
    {
        public BulkDeletionOperationContext(
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

            IBulkOperationState<IResourceIdentifier> receivedState = new BulkDeletionOperationState(request, operation, this);
            this.Initialize(receivedState);
        }
    }
}
