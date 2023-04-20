// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;
    using System.Net.Http;
    
    internal abstract class BulkOperationContextBase<TPayload> : IBulkOperationContext<TPayload> where TPayload : class
    {
        protected BulkOperationContextBase()
        {
        }

        public IRequest<BulkRequest2> BulkRequest => this.State.BulkRequest;

        public bool Completed
        {
            get
            {
                if (this.State == this.ProcessedState)
                {
                    return true;
                }

                if (this.State == this.FaultedState)
                {
                    return true;
                }

                return false;
            }
        }

        public bool Faulted
        {
            get
            {
                if (this.State == this.FaultedState)
                {
                    return true;
                }

                return false;
            }
        }

        public IBulkOperationState<TPayload> FaultedState
        {
            get;
            private set;
        }

        public HttpMethod Method => this.State.Operation.Method;

        public BulkRequestOperation Operation => this.State.Operation;

        public IBulkOperationState<TPayload> PreparedState
        {
            get;
            private set;
        }

        public IBulkOperationState<TPayload> ProcessedState
        {
            get;
            private set;
        }

        public IBulkOperationState<TPayload> ReceivedState
        {
            get;
            set;
        }

        public IRequest<TPayload> Request => this.State.Request;

        public BulkResponseOperation Response => this.State.Response;

        public IBulkOperationState<TPayload> State
        {
            get;
            set;
        }

        public void Complete(BulkResponseOperation response)
        {
            if (null == response)
            {
                throw new ArgumentNullException(nameof(response));
            }

            this.State.Complete(response);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "False analysis of the handling of receivedState.  It is assigned as the value of properties of the derived IBulkOperationState<TPayload> type, rather than of the base IBulkOperationState type.")]
        public void Initialize(IBulkOperationState<TPayload> receivedState)
        {
            if (null == receivedState)
            {
                throw new ArgumentNullException(nameof(receivedState));
            }

            if (null == receivedState.Operation)
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidState);
            }

            if (null == receivedState.BulkRequest)
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidState);
            }

            this.State = this.ReceivedState = receivedState;
            this.PreparedState = new BulkOperationState<TPayload>(receivedState.BulkRequest, receivedState.Operation, this);
            this.FaultedState = new BulkOperationState<TPayload>(receivedState.BulkRequest, receivedState.Operation, this);
            this.ProcessedState = new BulkOperationState<TPayload>(receivedState.BulkRequest, receivedState.Operation, this);
        }

        public void Prepare(IRequest<TPayload> request)
        {
            if (null == request)
            {
                throw new ArgumentNullException(nameof(request));
            }

            this.State.Prepare(request);
        }

        public bool TryPrepare() => this.State.TryPrepare();
    }
}
