// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    internal class BulkUpdateOperationState : BulkOperationStateBase<IPatch>, IBulkUpdateOperationState
    {
        private readonly List<IBulkCreationOperationContext> dependencies;
        private readonly IReadOnlyCollection<IBulkCreationOperationContext> dependenciesWrapper;

        public BulkUpdateOperationState(
            IRequest<BulkRequest2> request,
            BulkRequestOperation operation,
            IBulkOperationContext<IPatch> context)
            : base(request, operation, context)
        {
            this.dependencies = new List<IBulkCreationOperationContext>();
            this.dependenciesWrapper = this.dependencies.AsReadOnly();
        }

        public BulkUpdateOperationState(
            IRequest<BulkRequest2> request,
            BulkRequestOperation operation,
            IBulkOperationContext<IPatch> context,
            IBulkCreationOperationContext parent)
            : this(request, operation, context)
        {
            this.Parent = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        public IReadOnlyCollection<IBulkCreationOperationContext> Dependencies => this.dependenciesWrapper;
        
        public IBulkCreationOperationContext Parent
        {
            get;
            private set;
        }

        public void AddDependency(IBulkCreationOperationContext dependency)
        {
            if (null == dependency)
            {
                throw new ArgumentNullException(nameof(dependency));
            }

            if (this.Context.State != this.Context.ReceivedState)
            {
                throw new InvalidOperationException(
                    SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidState);
            }

            this.dependencies.Add(dependency);
        }

        public override void Complete(BulkResponseOperation response)
        {
            if (null == response)
            {
                throw new ArgumentNullException(nameof(response));
            }

            if (this.Context.State != this.Context.ReceivedState && this.Context.State != this.Context.PreparedState)
            {
                throw new InvalidOperationException(
                    SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidStateTransition);
            }

            IBulkOperationState<IPatch> completionState;
            if (response.Response is ErrorResponse)
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

                if (this.Parent != null)
                {
                    this.Parent.Complete(response);
                }
            }
            else
            {
                completionState.Complete(response);
            }
        }

        private void Fault(HttpStatusCode statusCode, ErrorType? errorType = null)
        {
            ErrorResponse error =
                        new ErrorResponse()
                        {
                            Status = statusCode
                        };
            if (errorType.HasValue)
            {
                error.ErrorType = errorType.Value;
            }
            BulkResponseOperation response =
                new BulkResponseOperation(this.Operation.Identifier)
                {
                    Response = error
                };
            this.Complete(response);
        }

        public override bool TryPrepareRequest(out IRequest<IPatch> request)
        {
            request = null;

            PatchRequest2 patchRequest;
            switch (this.Operation.Data)
            {
                case PatchRequest2 patchrequest2:
                    patchRequest = patchrequest2;
                    break;
                default:
                    dynamic operationDataJson = JsonConvert.DeserializeObject(Operation.Data.ToString());
                    IReadOnlyCollection<PatchOperation2Combined> patchOperations =
                        operationDataJson.Operations.ToObject<List<PatchOperation2Combined>>();
                    patchRequest = new PatchRequest2(patchOperations);
                    break;
            }
            IPatch patch =
                new Patch()
                {
                    PatchRequest = patchRequest
                };
            IRequest<IPatch> requestBuffer =
                new UpdateRequest(
                    this.BulkRequest.Request,
                    patch,
                    this.BulkRequest.CorrelationIdentifier,
                    this.BulkRequest.Extensions);

            Uri resourceIdentifier;
            if (this.Parent != null)
            {
                if (null == this.Parent.Response || null == this.Parent.Response.Location)
                {
                    this.Fault(HttpStatusCode.NotFound, ErrorType.noTarget);
                    return false;
                }

                resourceIdentifier = this.Parent.Response.Location;
            }
            else
            {
                if (null == this.BulkRequest || null == this.BulkRequest.BaseResourceIdentifier)
                {
                    throw new InvalidOperationException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidState);
                }

                if (null == this.Operation.Path)
                {
                    this.Fault(HttpStatusCode.BadRequest);
                    return false;
                }

                resourceIdentifier = new Uri(this.BulkRequest.BaseResourceIdentifier, this.Operation.Path);
            }

            if
            (
                !UniformResourceIdentifier.TryParse(resourceIdentifier, this.BulkRequest.Extensions,
                out IUniformResourceIdentifier parsedIdentifier) || null == parsedIdentifier
                || null == parsedIdentifier.Identifier
            )
            {
                this.Fault(HttpStatusCode.BadRequest);
                return false;
            }

            requestBuffer.Payload.ResourceIdentifier = parsedIdentifier.Identifier;

            if (this.Dependencies.Any())
            {
                foreach (IBulkCreationOperationContext dependency in this.Dependencies)
                {
                    if
                    (
                            null == dependency.Response
                        || null == dependency.Response.Location
                        || !UniformResourceIdentifier.TryParse(
                                dependency.Response.Location,
                                this.BulkRequest.Extensions,
                                out IUniformResourceIdentifier dependentResourceIdentifier)
                        || null == dependentResourceIdentifier.Identifier
                        || string.IsNullOrWhiteSpace(dependentResourceIdentifier.Identifier.Identifier)
                    )
                    {
                        this.Fault(HttpStatusCode.NotFound, ErrorType.noTarget);
                        return false;
                    }

                    if
                    (
                        !patchRequest.TryFindReference(
                            dependency.Operation.Identifier,
                            out IReadOnlyCollection<OperationValue> references)
                    )
                    {
                        this.Fault(HttpStatusCode.InternalServerError);
                        return false;
                    }

                    foreach (OperationValue value in references)
                    {
                        value.Value = dependentResourceIdentifier.Identifier.Identifier;
                    }
                }
            }

            request = requestBuffer;
            return true;
        }
    }
}
