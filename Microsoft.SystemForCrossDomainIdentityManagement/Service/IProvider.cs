// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Microsoft.AspNetCore.Builder;

    public interface IProvider
    {
        bool AcceptLargeObjects { get; set; }
        ServiceConfigurationBase Configuration { get; }
        //IEventTokenHandler EventHandler { get; set; }
        IReadOnlyCollection<IExtension> Extensions { get; }
        IResourceJsonDeserializingFactory<GroupBase> GroupDeserializationBehavior { get; }
        ISchematizedJsonDeserializingFactory<PatchRequest2> PatchRequestDeserializationBehavior { get; }
        IReadOnlyCollection<Core2ResourceType> ResourceTypes { get; }
        IReadOnlyCollection<TypeScheme> Schema { get; }
        //Action<IApplicationBuilder, HttpConfiguration> StartupBehavior { get; }
        IResourceJsonDeserializingFactory<Core2UserBase> UserDeserializationBehavior { get; }
        Task<Resource> CreateAsync(IRequest<Resource> request);
        Task DeleteAsync(IRequest<IResourceIdentifier> request);
        Task<QueryResponseBase> PaginateQueryAsync(IRequest<IQueryParameters> request);
        Task<Resource[]> QueryAsync(IRequest<IQueryParameters> request);
        Task<Resource> ReplaceAsync(IRequest<Resource> request);
        Task<Resource> RetrieveAsync(IRequest<IResourceRetrievalParameters> request);
        Task UpdateAsync(IRequest<IPatch> request);
        Task<BulkResponse2> ProcessAsync(IRequest<BulkRequest2> request);
    }
}
