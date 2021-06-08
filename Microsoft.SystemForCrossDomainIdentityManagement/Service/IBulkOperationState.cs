//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    public interface IBulkOperationState
    {
        IRequest<BulkRequest2> BulkRequest { get; }
        BulkRequestOperation Operation { get; }
        BulkResponseOperation Response { get; }

        void Complete(BulkResponseOperation response);
        bool TryPrepare();
    }

    public interface IBulkOperationState<TPayload> : IBulkOperationState where TPayload : class
    {
        void Prepare(IRequest<TPayload> request);
        IRequest<TPayload> Request { get; }
    }
}
