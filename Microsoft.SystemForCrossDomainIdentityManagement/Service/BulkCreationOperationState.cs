
namespace Microsoft.SCIM
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    internal class BulkCreationOperationState : BulkOperationStateBase<Resource>, IBulkCreationOperationState
    {
        private const string RelativeResourceIdentifierTemplate = "/{0}/{1}";

        private readonly List<IBulkUpdateOperationContext> dependents;
        private readonly IReadOnlyCollection<IBulkUpdateOperationContext> dependentsWrapper;

        private readonly IRequest<Resource> creationRequest;

        private readonly List<IBulkUpdateOperationContext> subordinates;
        private readonly IReadOnlyCollection<IBulkUpdateOperationContext> subordinatesWrapper;

        private readonly IBulkCreationOperationContext typedContext;

        public BulkCreationOperationState(
            IRequest<BulkRequest2> request,
            BulkRequestOperation operation,
            IBulkCreationOperationContext context)
            : base(request, operation, context)
        {
            this.typedContext = context;

            this.dependents = new List<IBulkUpdateOperationContext>();
            this.dependentsWrapper = this.dependents.AsReadOnly();

            this.subordinates = new List<IBulkUpdateOperationContext>();
            this.subordinatesWrapper = this.subordinates.AsReadOnly();

            if (null == this.BulkRequest.BaseResourceIdentifier)
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidRequest);
            }

            if (null == this.Operation.Data)
            {
                string invalidOperationExceptionMessage =
                    string.Format(
                        CultureInfo.InvariantCulture,
                        SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidOperationTemplate,
                        operation.Identifier);
                throw new ArgumentException(invalidOperationExceptionMessage);
            }

            dynamic operationDataJson = JsonConvert.DeserializeObject(operation.Data.ToString());

            if (null != operationDataJson.schemas)
            {
                object operationData = operation.Data;
                Resource resource = null;

                if (operationData.IsResourceType(SchemaIdentifiers.Core2User))
                {
                    Core2EnterpriseUserBase user = operationDataJson.ToObject<Core2EnterpriseUser>();
                    resource = user;
                    if (user.EnterpriseExtension.Manager != null)
                    {
                        string resourceIdentifier =
                            string.Format(
                                CultureInfo.InvariantCulture,
                                BulkCreationOperationState.RelativeResourceIdentifierTemplate,
                                ProtocolConstants.PathUsers,
                                this.Operation.Identifier);
                        Uri patchResourceIdentifier =
                            new Uri(resourceIdentifier, UriKind.Relative);

                        PatchOperation2Combined patchOperation =
                            PatchOperation2Combined.Create(
                                OperationName.Add,
                                AttributeNames.Manager,
                                user.EnterpriseExtension.Manager.Value);
                        PatchRequest2 patchRequest = new PatchRequest2();
                        patchRequest.AddOperation(patchOperation);

                        this.AddSubordinate(patchResourceIdentifier, patchRequest, context);

                        user.EnterpriseExtension.Manager = null;
                    }
                }

                if (operationData.IsResourceType(SchemaIdentifiers.Core2Group))
                {
                    GroupBase group = operationDataJson.ToObject<Core2Group>();
                    resource = group;
                    if (group.Members != null && group.Members.Any())
                    {
                        string resourceIdentifier =
                            string.Format(
                                CultureInfo.InvariantCulture,
                                BulkCreationOperationState.RelativeResourceIdentifierTemplate,
                                ProtocolConstants.PathGroups,
                                this.Operation.Identifier);
                        Uri patchResourceIdentifier =
                            new Uri(resourceIdentifier, UriKind.Relative);

                        PatchRequest2 patchRequest = new PatchRequest2();
                        foreach (Member member in group.Members)
                        {
                            if (member == null || string.IsNullOrWhiteSpace(member.Value))
                            {
                                continue;
                            }

                            string memberValue = System.Text.Json.JsonSerializer.Serialize(member);
                            if (!string.IsNullOrWhiteSpace(memberValue))
                            {
                                PatchOperation2Combined patchOperation =
                                    PatchOperation2Combined.Create(
                                        OperationName.Add,
                                        AttributeNames.Members,
                                        memberValue);
                                patchRequest.AddOperation(patchOperation);
                            }
                        }

                        this.AddSubordinate(patchResourceIdentifier, patchRequest, context);

                        group.Members = null;
                    }
                }
                if (null == resource)
                {
                    string invalidOperationExceptionMessage = 
                        string.Format(
                            CultureInfo.InvariantCulture,
                            SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidOperationTemplate,
                            operation.Identifier);
                    throw new ArgumentException(invalidOperationExceptionMessage);
                }
                this.creationRequest =
                        new CreationRequest(request.Request, resource, request.CorrelationIdentifier,
                        request.Extensions);
            }
            else
            {
                string invalidOperationExceptionMessage =
                    string.Format(
                        CultureInfo.InvariantCulture,
                        SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidOperationTemplate,
                        operation.Identifier);
                throw new ArgumentException(invalidOperationExceptionMessage);
            }
        }

        public IReadOnlyCollection<IBulkUpdateOperationContext> Dependents => this.dependentsWrapper;
       
        public IReadOnlyCollection<IBulkUpdateOperationContext> Subordinates => this.subordinatesWrapper;
       
        public void AddDependent(IBulkUpdateOperationContext dependent)
        {
            if (null == dependent)
            {
                throw new ArgumentNullException(nameof(dependent));
            }

            if (this.Context.State != this.Context.ReceivedState)
            {
                throw new InvalidOperationException(
                    SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidState);
            }

            this.dependents.Add(dependent);
        }

        public void AddSubordinate(IBulkUpdateOperationContext subordinate)
        {
            if (null == subordinate)
            {
                throw new ArgumentNullException(nameof(subordinate));
            }

            if (this.Context.State != this.Context.ReceivedState)
            {
                throw new InvalidOperationException(
                    SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidState);
            }

            this.subordinates.Add(subordinate);
        }

        private void AddSubordinate(Uri resourceIdentifier, PatchRequest2 patchRequest, IBulkCreationOperationContext context)
        {
            if (null == resourceIdentifier)
            {
                throw new ArgumentNullException(nameof(resourceIdentifier));
            }

            if (null == patchRequest)
            {
                throw new ArgumentNullException(nameof(patchRequest));
            }

            if (null == context)
            {
                throw new ArgumentNullException(nameof(context));
            }

            BulkRequestOperation bulkPatchOperation =
                BulkRequestOperation.CreatePatchOperation(resourceIdentifier, patchRequest);
            IBulkUpdateOperationContext patchOperationContext =
                new BulkUpdateOperationContext(this.BulkRequest, bulkPatchOperation, context);
            this.AddSubordinate(patchOperationContext);
        }

        public override void Complete(BulkResponseOperation response)
        {
            if (null == response)
            {
                throw new ArgumentNullException(nameof(response));
            }

            if (!this.typedContext.Subordinates.Any())
            {
                base.Complete(response);
                return;
            }

            if
            (
                    this.Context.State != this.Context.PreparedState
                && this.typedContext.State != this.typedContext.PendingState
            )
            {
                throw new InvalidOperationException(
                    SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidStateTransition);
            }

            IBulkOperationState<Resource> nextState;
            if (response.Response is ErrorResponse)
            {
                nextState = this.Context.FaultedState;
            }
            else if (this.typedContext.Subordinates.Any((IBulkUpdateOperationContext item) => !item.Completed))
            {
                nextState = this.typedContext.PendingState;
            }
            else
            {
                nextState = this.Context.ProcessedState;
            }

            if (this == nextState)
            {
                if (this != this.typedContext.PendingState || null == this.Response)
                {
                    this.Response = response;
                }
                this.Context.State = this;
            }
            else
            {
                nextState.Complete(response);
            }
        }

        public override bool TryPrepareRequest(out IRequest<Resource> request)
        {
            request = this.creationRequest;
            return true;
        }
    }
}
