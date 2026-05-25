// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;
    using System.Net.Http;

    internal class InvalidBulkOperationContext : IBulkOperationContext
    {
        private readonly IBulkOperationState state;

        public InvalidBulkOperationContext(
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

            this.state = new InvalidBulkOperationState(request, operation);
        }

        public bool Completed => true;

        public bool Faulted => true;
        
        public IRequest<BulkRequest2> BulkRequest => this.state.BulkRequest;

        public HttpMethod Method => this.state.Operation.Method;
        
        public BulkRequestOperation Operation => this.state.Operation;
        
        public BulkResponseOperation Response => this.state.Response;

        public void Complete(BulkResponseOperation response) => this.state.Complete(response);

        public bool TryPrepare() => this.state.TryPrepare();
    }
}
