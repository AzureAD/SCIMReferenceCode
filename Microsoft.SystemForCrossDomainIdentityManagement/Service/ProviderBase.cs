// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    public abstract class ProviderBase : IProvider
    {
        private static readonly Lazy<BulkRequestsFeature> BulkFeatureSupport =
            new Lazy<BulkRequestsFeature>(
                () =>
                    BulkRequestsFeature.CreateUnsupportedFeature());

        private static readonly Lazy<IReadOnlyCollection<TypeScheme>> TypeSchema =
            new Lazy<IReadOnlyCollection<TypeScheme>>(
                () =>
                    Array.Empty<TypeScheme>());

        private static readonly Lazy<ServiceConfigurationBase> ServiceConfiguration =
            new Lazy<ServiceConfigurationBase>(
                () =>
                    new Core2ServiceConfiguration(ProviderBase.BulkFeatureSupport.Value, false, true, false, true, false));

        private static readonly Lazy<IReadOnlyCollection<Core2ResourceType>> Types =
            new Lazy<IReadOnlyCollection<Core2ResourceType>>(
                () =>
                    Array.Empty<Core2ResourceType>());

        public virtual bool AcceptLargeObjects
        {
            get;
            set;
        }

        public virtual ServiceConfigurationBase Configuration
        {
            get
            {
                return ProviderBase.ServiceConfiguration.Value;
            }
        }

        //public virtual IEventTokenHandler EventHandler
        //{
        //    get;
        //    set;
        //}

        public virtual IReadOnlyCollection<IExtension> Extensions
        {
            get
            {
                return null;
            }
        }

        public virtual IResourceJsonDeserializingFactory<GroupBase> GroupDeserializationBehavior
        {
            get
            {
                return null;
            }
        }

        public virtual ISchematizedJsonDeserializingFactory<PatchRequest2> PatchRequestDeserializationBehavior
        {
            get
            {
                return null;
            }
        }

        public virtual IReadOnlyCollection<Core2ResourceType> ResourceTypes
        {
            get
            {
                return ProviderBase.Types.Value;
            }
        }

        public virtual IReadOnlyCollection<TypeScheme> Schema
        {
            get
            {
                return ProviderBase.TypeSchema.Value;
            }
        }

        //public virtual Action<IAppBuilder, HttpConfiguration> StartupBehavior
        //{
        //    get
        //    {
        //        return null;
        //    }
        //}

        public virtual IResourceJsonDeserializingFactory<Core2UserBase> UserDeserializationBehavior
        {
            get
            {
                return null;
            }
        }

        public abstract Task<Resource> CreateAsync(Resource resource, string correlationIdentifier);

        public virtual async Task<Resource> CreateAsync(IRequest<Resource> request)
        {
            if (null == request)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (null == request.Payload)
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidRequest);
            }

            if (string.IsNullOrWhiteSpace(request.CorrelationIdentifier))
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidRequest);
            }

            Resource result = await this.CreateAsync(request.Payload, request.CorrelationIdentifier).ConfigureAwait(false);
            return result;
        }

        public abstract Task DeleteAsync(IResourceIdentifier resourceIdentifier, string correlationIdentifier);

        public virtual async Task DeleteAsync(IRequest<IResourceIdentifier> request)
        {
            if (null == request)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (null == request.Payload)
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidRequest);
            }

            if (string.IsNullOrWhiteSpace(request.CorrelationIdentifier))
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidRequest);
            }

            await this.DeleteAsync(request.Payload, request.CorrelationIdentifier).ConfigureAwait(false);
        }

        public virtual async Task<QueryResponseBase> PaginateQueryAsync(IRequest<IQueryParameters> request)
        {
            if (null == request)
            {
                throw new ArgumentNullException(nameof(request));
            }

            IReadOnlyCollection<Resource> resources = await this.QueryAsync(request).ConfigureAwait(false);
            QueryResponseBase result = new QueryResponse(resources);
            result.TotalResults =
                result.ItemsPerPage =
                    resources.Count;
            result.StartIndex = resources.Any() ? 1 : (int?)null;
            return result;
        }


        public  virtual async Task<BulkResponse2> ProcessAsync(IRequest<BulkRequest2> request)
        {
            if (null == request)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (null == request.Request)
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidRequest);
            }
            Queue<IBulkOperationContext> operations = request.EnqueueOperations();
            BulkResponse2 result = await this.ProcessAsync(operations).ConfigureAwait(false);
            return result;
        }

        public virtual async Task ProcessAsync(IBulkOperationContext operation)
        {
            if (null == operation)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            if (!operation.TryPrepare())
            {
                return;
            }

            if (null == operation.Method)
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidOperation);
            }

            if (null == operation.Operation)
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidOperation);
            }

            BulkResponseOperation response =
                new BulkResponseOperation(operation.Operation.Identifier)
                {
                    Method = operation.Method
                };

            if (HttpMethod.Delete == operation.Method)
            {
                IBulkOperationContext<IResourceIdentifier> context = (IBulkOperationContext<IResourceIdentifier>)operation;
                await this.DeleteAsync(context.Request).ConfigureAwait(false);
                response.Status = HttpStatusCode.NoContent;
            }
            else if (HttpMethod.Get == operation.Method)
            {
                switch (operation)
                {
                    case IBulkOperationContext<IResourceRetrievalParameters> retrievalContext:
                        response.Response = await this.RetrieveAsync(retrievalContext.Request).ConfigureAwait(false);
                        break;
                    default:
                        IBulkOperationContext<IQueryParameters> queryContext = (IBulkOperationContext<IQueryParameters>)operation;
                        response.Response = await this.QueryAsync(queryContext.Request).ConfigureAwait(false);
                        break;
                }
                response.Status = HttpStatusCode.OK;
            }
            else if (ProtocolExtensions.PatchMethod == operation.Method)
            {
                IBulkOperationContext<IPatch> context = (IBulkOperationContext<IPatch>)operation;
                await this.UpdateAsync(context.Request).ConfigureAwait(false);
                response.Status = HttpStatusCode.OK;
            }
            else if (HttpMethod.Post == operation.Method)
            {
                IBulkOperationContext<Resource> context = (IBulkOperationContext<Resource>)operation;
                Resource output = await this.CreateAsync(context.Request).ConfigureAwait(false);
                response.Status = HttpStatusCode.Created;
                response.Location = output.GetResourceIdentifier(context.BulkRequest.BaseResourceIdentifier);
            }
            else
            {
                string exceptionMessage =
                    string.Format(
                        CultureInfo.InvariantCulture,
                        SystemForCrossDomainIdentityManagementServiceResources.ExceptionMethodNotSupportedTemplate,
                        operation.Method);
                ErrorResponse error =
                    new ErrorResponse()
                    {
                        Status = HttpStatusCode.BadRequest,
                        Detail = exceptionMessage
                    };
                response.Response = error;
                response.Status = HttpStatusCode.BadRequest;
            }

            operation.Complete(response);
        }

        public virtual async Task<BulkResponse2> ProcessAsync(Queue<IBulkOperationContext> operations)
        {
            if (null == operations)
            {
                throw new ArgumentNullException(nameof(operations));
            }

            BulkResponse2 result = new BulkResponse2();
            int countFailures = 0;
            while (operations.Any())
            {
                IBulkOperationContext operation = operations.Dequeue();
                await this.ProcessAsync(operation).ConfigureAwait(false);

                bool addOperation;
                switch (operation)
                {
                    case IBulkUpdateOperationContext updateOperation:
                        addOperation = null == updateOperation.Parent;
                        break;
                    default:
                        addOperation = true;
                        break;
                }
                if (addOperation)
                {
                    result.AddOperation(operation.Response);
                }

                if (operation.Response.IsError())
                {
                    checked
                    {
                        countFailures++;
                    }
                }

                if
                (
                        operation.BulkRequest.Payload.FailOnErrors.HasValue
                    && countFailures > operation.BulkRequest.Payload.FailOnErrors.Value
                )
                {
                    break;
                }
            }
            return result;
        }

        public virtual Task<Resource[]> QueryAsync(IQueryParameters parameters, string correlationIdentifier)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<Resource[]> QueryAsync(IRequest<IQueryParameters> request)
        {
            if (null == request)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (null == request.Payload)
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidRequest);
            }

            if (string.IsNullOrWhiteSpace(request.CorrelationIdentifier))
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidRequest);
            }

            Resource[] result = await this.QueryAsync(request.Payload, request.CorrelationIdentifier).ConfigureAwait(false);
            return result;
        }

        public virtual Task<Resource> ReplaceAsync(Resource resource, string correlationIdentifier)
        {
            throw new NotSupportedException();
        }

        public virtual async Task<Resource> ReplaceAsync(IRequest<Resource> request)
        {
            if (null == request)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (null == request.Payload)
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidRequest);
            }

            if (string.IsNullOrWhiteSpace(request.CorrelationIdentifier))
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidRequest);
            }

            Resource result = await this.ReplaceAsync(request.Payload, request.CorrelationIdentifier).ConfigureAwait(false);
            return result;
        }

        public abstract Task<Resource> RetrieveAsync(IResourceRetrievalParameters parameters, string correlationIdentifier);

        public virtual async Task<Resource> RetrieveAsync(IRequest<IResourceRetrievalParameters> request)
        {
            if (null == request)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (null == request.Payload)
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidRequest);
            }

            if (string.IsNullOrWhiteSpace(request.CorrelationIdentifier))
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidRequest);
            }

            Resource result = await this.RetrieveAsync(request.Payload, request.CorrelationIdentifier).ConfigureAwait(false);
            return result;
        }

        public abstract Task UpdateAsync(IPatch patch, string correlationIdentifier);

        public virtual async Task UpdateAsync(IRequest<IPatch> request)
        {
            if (null == request)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (null == request.Payload)
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidRequest);
            }

            if (string.IsNullOrWhiteSpace(request.CorrelationIdentifier))
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidRequest);
            }

            await this.UpdateAsync(request.Payload, request.CorrelationIdentifier).ConfigureAwait(false);
        }
    }
}
