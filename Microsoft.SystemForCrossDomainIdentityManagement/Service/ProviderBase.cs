//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;

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