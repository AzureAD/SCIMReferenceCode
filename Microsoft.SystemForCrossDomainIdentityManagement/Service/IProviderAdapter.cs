// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM
{
    using Microsoft.AspNetCore.Http;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IProviderAdapter<T> where T : Resource
    {
        string SchemaIdentifier { get; }

        Task<Resource> Create(HttpContext httpContext, Resource resource, string correlationIdentifier);
        Task<Resource> Delete(HttpContext httpContext, string identifier, string correlationIdentifier);
        Task<QueryResponseBase> Query(
            HttpContext httpContext,
            IReadOnlyCollection<IFilter> filters,
            IReadOnlyCollection<string> requestedAttributePaths,
            IReadOnlyCollection<string> excludedAttributePaths,
            IPaginationParameters paginationParameters,
            string correlationIdentifier);
        Task<Resource> Replace(HttpContext httpContext, Resource resource, string correlationIdentifier);
        Task<Resource> Retrieve(
            HttpContext httpContext,
            string identifier,
            IReadOnlyCollection<string> requestedAttributePaths,
            IReadOnlyCollection<string> excludedAttributePaths,
            string correlationIdentifier);
        Task<Resource> Update(
            HttpContext httpContext,
            string identifier,
            PatchRequestBase patchRequest,
            string correlationIdentifier);
    }
}
