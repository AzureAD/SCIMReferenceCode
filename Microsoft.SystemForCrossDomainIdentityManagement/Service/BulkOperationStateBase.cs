// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;
    
    internal abstract class BulkOperationStateBase<TPayload> : IBulkOperationState<TPayload> where TPayload : class
    {
        protected BulkOperationStateBase(
            IRequest<BulkRequest2> request,
            BulkRequestOperation operation,
            IBulkOperationContext<TPayload> context)
        {
            this.BulkRequest = request ?? throw new ArgumentNullException(nameof(request));
            this.Operation = operation ?? throw new ArgumentNullException(nameof(operation));
            this.Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IRequest<BulkRequest2> BulkRequest
        {
            get;
            private set;
        }

        public IBulkOperationContext<TPayload> Context
        {
            get;
            private set;
        }

        public BulkRequestOperation Operation
        {
            get;
            set;
        }

        public IRequest<TPayload> Request
        {
            get;
            private set;
        }

        public BulkResponseOperation Response
        {
            get;
            set;
        }

        public virtual void Complete(BulkResponseOperation response)
        {
            if (null == response)
            {
                throw new ArgumentNullException(nameof(response));
            }

            ErrorResponse errorResponse = response.Response as ErrorResponse;
            if (this.Context.State != this.Context.PreparedState && null == errorResponse)
            {
                throw new InvalidOperationException(
                    SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidStateTransition);
            }

            IBulkOperationState<TPayload> completionState;
            if (errorResponse != null)
            {
                completionState = this.Context.FaultedState;
            }
            else
            {
                completionState = this.Context.ProcessedState;
            }

            if (this == completionState)
            {
                this.Response = response;
                this.Context.State = this;
            }
            else
            {
                completionState.Complete(response);
            }
        }

        public virtual void Prepare(IRequest<TPayload> request)
        {
            if (this.Context.State != this.Context.ReceivedState)
            {
                throw new InvalidOperationException(
                    SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidStateTransition);
            }

            if (this != this.Context.PreparedState)
            {
                throw new InvalidOperationException(
                    SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidStateTransition);
            }

            this.Request = request ?? throw new ArgumentNullException(nameof(request));
            this.Context.State = this;
        }

        public virtual bool TryPrepare()
        {
            if (this.Context.State != this.Context.ReceivedState)
            {
                throw new InvalidOperationException(
                    SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidStateTransition);
            }

            if (!this.TryPrepareRequest(out IRequest<TPayload> request))
            {
                if (this.Context.State != this.Context.FaultedState)
                {
                    this.Context.State = this.Context.FaultedState;
                }

                return false;
            }

            this.Context.PreparedState.Prepare(request);
            return true;
        }

        public abstract bool TryPrepareRequest(out IRequest<TPayload> request);
    }
}
