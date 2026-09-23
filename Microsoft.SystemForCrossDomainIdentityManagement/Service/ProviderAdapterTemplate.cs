// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    public abstract class ProviderAdapterTemplate<T> : IProviderAdapter<T> where T : Resource
    {
        protected ProviderAdapterTemplate(IProvider provider)
        {
            this.Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public IProvider Provider
        {
            get;
            set;
        }

        public abstract string SchemaIdentifier { get; }

        public virtual async Task<Resource> Create(HttpContext httpContext, Resource resource, string correlationIdentifier)
        {
            if (null == httpContext)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (null == resource)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            if (string.IsNullOrWhiteSpace(correlationIdentifier))
            {
                throw new ArgumentNullException(nameof(correlationIdentifier));
            }

            IReadOnlyCollection<IExtension> extensions = this.ReadExtensions();
            IRequest<Resource> creationRequest = new SystemForCrossDomainIdentityManagementRequest<Resource>(httpContext, resource, correlationIdentifier, extensions);
            Resource result = await this.Provider.CreateAsync(creationRequest).ConfigureAwait(false);
            return result;
        }

        public virtual IResourceIdentifier CreateResourceIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            IResourceIdentifier result =
                new ResourceIdentifier()
                {
                    Identifier = identifier,
                    SchemaIdentifier = this.SchemaIdentifier
                };
            return result;
        }

        public virtual async Task<Resource> Delete(HttpContext httpContext, string identifier, string correlationIdentifier)
        {
            if (null == httpContext)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (string.IsNullOrWhiteSpace(identifier))
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            if (string.IsNullOrWhiteSpace(correlationIdentifier))
            {
                throw new ArgumentNullException(nameof(correlationIdentifier));
            }

            IReadOnlyCollection<IExtension> extensions = this.ReadExtensions();
            IResourceIdentifier resourceIdentifier = this.CreateResourceIdentifier(identifier);
            IRequest<IResourceIdentifier> deletionRequest =
                new SystemForCrossDomainIdentityManagementRequest<IResourceIdentifier>(httpContext, resourceIdentifier, correlationIdentifier, extensions);
            return await this.Provider.DeleteAsync(deletionRequest).ConfigureAwait(false);
        }

        public virtual string GetPath()
        {
            IReadOnlyCollection<IExtension> extensions = this.ReadExtensions();
            if (extensions != null && extensions.TryGetPath(this.SchemaIdentifier, out string result))
            {
                return result;
            }

            result = new SchemaIdentifier(this.SchemaIdentifier).FindPath();
            return result;
        }

        public virtual async Task<QueryResponseBase> Query(
            HttpContext httpContext,
            IReadOnlyCollection<IFilter> filters,
            IReadOnlyCollection<string> requestedAttributePaths,
            IReadOnlyCollection<string> excludedAttributePaths,
            IPaginationParameters paginationParameters,
            string correlationIdentifier)
        {
            if (null == httpContext)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (null == requestedAttributePaths)
            {
                throw new ArgumentNullException(nameof(requestedAttributePaths));
            }

            if (null == excludedAttributePaths)
            {
                throw new ArgumentNullException(nameof(excludedAttributePaths));
            }

            if (string.IsNullOrWhiteSpace(correlationIdentifier))
            {
                throw new ArgumentNullException(nameof(correlationIdentifier));
            }

            string path = this.GetPath();
            IQueryParameters queryParameters =
                new QueryParameters(this.SchemaIdentifier, path, filters, requestedAttributePaths, excludedAttributePaths);
            queryParameters.PaginationParameters = paginationParameters;
            IReadOnlyCollection<IExtension> extensions = this.ReadExtensions();
            IRequest<IQueryParameters> queryRequest =
                new SystemForCrossDomainIdentityManagementRequest<IQueryParameters>(httpContext, queryParameters, correlationIdentifier, extensions);
            QueryResponseBase result = await this.Provider.PaginateQueryAsync(queryRequest).ConfigureAwait(false);

            return result;
        }

        private IReadOnlyCollection<IExtension> ReadExtensions()
        {
            IReadOnlyCollection<IExtension> result;
            try
            {
                result = this.Provider.Extensions;
            }
            catch (NotImplementedException)
            {
                result = null;
            }
            return result;
        }

        public virtual async Task<Resource> Replace(
            HttpContext httpContext,
            Resource resource,
            string correlationIdentifier)
        {
            if (null == httpContext)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (null == resource)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            if (string.IsNullOrWhiteSpace(correlationIdentifier))
            {
                throw new ArgumentNullException(nameof(correlationIdentifier));
            }

            IReadOnlyCollection<IExtension> extensions = this.ReadExtensions();
            IRequest<Resource> replaceRequest = new SystemForCrossDomainIdentityManagementRequest<Resource>(httpContext, resource, correlationIdentifier, extensions);
            Resource result = await this.Provider.ReplaceAsync(replaceRequest).ConfigureAwait(false);
            return result;
        }

        public virtual async Task<Resource> Retrieve(
            HttpContext httpContext,
            string identifier,
            IReadOnlyCollection<string> requestedAttributePaths,
            IReadOnlyCollection<string> excludedAttributePaths,
            string correlationIdentifier)
        {
            if (null == httpContext)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (string.IsNullOrWhiteSpace(identifier))
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            if (null == requestedAttributePaths)
            {
                throw new ArgumentNullException(nameof(requestedAttributePaths));
            }

            if (string.IsNullOrWhiteSpace(correlationIdentifier))
            {
                throw new ArgumentNullException(nameof(correlationIdentifier));
            }

            string path = this.GetPath();
            IResourceRetrievalParameters retrievalParameters =
                new ResourceRetrievalParameters(
                        this.SchemaIdentifier,
                        path,
                        identifier,
                        requestedAttributePaths,
                        excludedAttributePaths);
            IReadOnlyCollection<IExtension> extensions = this.ReadExtensions();
            IRequest<IResourceRetrievalParameters> retrievalRequest =
                new SystemForCrossDomainIdentityManagementRequest<IResourceRetrievalParameters>(httpContext, retrievalParameters, correlationIdentifier, extensions);
            Resource result = await this.Provider.RetrieveAsync(retrievalRequest).ConfigureAwait(false);
            return result;
        }

        public virtual async Task<Resource> Update(HttpContext httpContext,
            string identifier,
            PatchRequestBase patchRequest,
            string correlationIdentifier)
        {
            if (null == httpContext)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (string.IsNullOrWhiteSpace(correlationIdentifier))
            {
                throw new ArgumentNullException(nameof(correlationIdentifier));
            }

            IResourceIdentifier resourceIdentifier = this.CreateResourceIdentifier(identifier);
            IPatch patch =
                new Patch
                {
                    ResourceIdentifier = resourceIdentifier,
                    PatchRequest = patchRequest
                };
            IReadOnlyCollection<IExtension> extensions = this.ReadExtensions();
            IRequest<IPatch> updateRequest = new SystemForCrossDomainIdentityManagementRequest<IPatch>(httpContext, patch, correlationIdentifier, extensions);
            return await this.Provider.UpdateAsync(updateRequest).ConfigureAwait(false);
        }
    }
}
