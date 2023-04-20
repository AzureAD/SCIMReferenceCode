// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;

    internal class InvalidBulkOperationState : IBulkOperationState
    {
        public InvalidBulkOperationState(
            IRequest<BulkRequest2> request,
            BulkRequestOperation operation)
        {
            this.BulkRequest = request ?? throw new ArgumentNullException(nameof(request));
            this.Operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public IRequest<BulkRequest2> BulkRequest
        {
            get;
            private set;
        }

        public BulkRequestOperation Operation
        {
            get;
            private set;
        }

        public BulkResponseOperation Response
        {
            get;
            private set;
        }

        public void Complete(BulkResponseOperation response)
        {
            if (null == response)
            {
                throw new ArgumentNullException(nameof(response));
            }

            if (response.Response is ErrorResponse)
            {
                this.Response = response;
            }
            else
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidResponse);
            }
        }

        public bool TryPrepare() => false;
    }
}
