// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM.WebHostSample.Provider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Microsoft.SCIM;

    public class InMemoryGroupProvider : ProviderBase
    {
        private readonly InMemoryStorage storage;

        public InMemoryGroupProvider()
        {
            this.storage = InMemoryStorage.Instance;
        }

        public override Task<Resource> CreateAsync(Resource resource, string correlationIdentifier)
        {
            if (resource.Identifier != null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            Core2Group group = resource as Core2Group;

            if (string.IsNullOrWhiteSpace(group.DisplayName))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            IEnumerable<Core2Group> exisitingGroups = this.storage.Groups.Values;
            if
            (
                exisitingGroups.Any(
                    (Core2Group exisitingGroup) =>
                        string.Equals(exisitingGroup.DisplayName, group.DisplayName, StringComparison.Ordinal))
            )
            {
                throw new HttpResponseException(HttpStatusCode.Conflict);
            }
            //Update Metadata
            DateTime created = DateTime.UtcNow;
            group.Metadata.Created = created;
            group.Metadata.LastModified = created;

            string resourceIdentifier = Guid.NewGuid().ToString();
            resource.Identifier = resourceIdentifier;
            this.storage.Groups.Add(resourceIdentifier, group);

            return Task.FromResult(resource);
        }

        public override Task DeleteAsync(IResourceIdentifier resourceIdentifier, string correlationIdentifier)
        {
            if (string.IsNullOrWhiteSpace(resourceIdentifier?.Identifier))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            string identifier = resourceIdentifier.Identifier;

            if (this.storage.Groups.ContainsKey(identifier))
            {
                this.storage.Groups.Remove(identifier);
            }

            return Task.CompletedTask;
        }

        public override Task<Resource[]> QueryAsync(IQueryParameters parameters, string correlationIdentifier)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (string.IsNullOrWhiteSpace(correlationIdentifier))
            {
                throw new ArgumentNullException(nameof(correlationIdentifier));
            }

            if (null == parameters.AlternateFilters)
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidParameters);
            }

            if (string.IsNullOrWhiteSpace(parameters.SchemaIdentifier))
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidParameters);
            }

            IEnumerable<Resource> results;
            IFilter queryFilter = parameters.AlternateFilters.SingleOrDefault();

            var predicate = PredicateBuilder.False<Core2Group>();
            Expression<Func<Core2Group, bool>> predicateAnd;
            predicateAnd = PredicateBuilder.True<Core2Group>();

            if (queryFilter == null)
            {
                results = this.storage.Groups.Values.Select(
                    (Core2Group user) => user as Resource);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(queryFilter.AttributePath))
                {
                    throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidParameters);
                }

                if (string.IsNullOrWhiteSpace(queryFilter.ComparisonValue))
                {
                    throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidParameters);
                }

                if (queryFilter.FilterOperator != ComparisonOperator.Equals)
                {
                    throw new NotSupportedException(string.Format(SystemForCrossDomainIdentityManagementServiceResources.ExceptionFilterOperatorNotSupportedTemplate, queryFilter.FilterOperator));
                }


                if (queryFilter.AttributePath.Equals(AttributeNames.DisplayName))
                {
                    
                    string displayName = queryFilter.ComparisonValue;
                    predicateAnd = predicateAnd.And(p => string.Equals(p.DisplayName, displayName, StringComparison.OrdinalIgnoreCase));
                  
                }
                else
                {
                    throw new NotSupportedException(string.Format(SystemForCrossDomainIdentityManagementServiceResources.ExceptionFilterAttributePathNotSupportedTemplate, queryFilter.AttributePath));
                }
            }
            
            predicate = predicate.Or(predicateAnd);
            results = this.storage.Groups.Values.Where(predicate.Compile());

            return Task.FromResult(results.ToArray());
        }

        public override Task<Resource> ReplaceAsync(Resource resource, string correlationIdentifier)
        {
            if (resource.Identifier == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            Core2Group group = resource as Core2Group;

            if (string.IsNullOrWhiteSpace(group.DisplayName))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            Core2Group exisitingGroups = resource as Core2Group;
            if
            (
                this.storage.Groups.Values.Any(
                    (Core2Group exisitingUser) =>
                        string.Equals(exisitingUser.DisplayName, group.DisplayName, StringComparison.Ordinal) &&
                        !string.Equals(exisitingUser.Identifier, group.Identifier, StringComparison.OrdinalIgnoreCase))
            )
            {
                throw new HttpResponseException(HttpStatusCode.Conflict);
            }

            if (!this.storage.Groups.TryGetValue(group.Identifier, out Core2Group _))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            // Update metadata
            group.Metadata.Created = exisitingGroups.Metadata.Created;
            group.Metadata.LastModified = DateTime.UtcNow;

            this.storage.Groups[group.Identifier] = group;
            Resource result = group as Resource;
            return Task.FromResult(result);
        }

        public override Task<Resource> RetrieveAsync(IResourceRetrievalParameters parameters, string correlationIdentifier)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (string.IsNullOrWhiteSpace(correlationIdentifier))
            {
                throw new ArgumentNullException(nameof(correlationIdentifier));
            }

            if (string.IsNullOrEmpty(parameters?.ResourceIdentifier?.Identifier))
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            string identifier = parameters.ResourceIdentifier.Identifier;

            if (this.storage.Groups.ContainsKey(identifier))
            {
                if (this.storage.Groups.TryGetValue(identifier, out Core2Group group))
                {
                    Resource result = group as Resource;
                    return Task.FromResult(result);
                }
            }

            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        public override Task UpdateAsync(IPatch patch, string correlationIdentifier)
        {
            if (null == patch)
            {
                throw new ArgumentNullException(nameof(patch));
            }

            if (null == patch.ResourceIdentifier)
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidOperation);
            }

            if (string.IsNullOrWhiteSpace(patch.ResourceIdentifier.Identifier))
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidOperation);
            }

            if (null == patch.PatchRequest)
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidOperation);
            }

            PatchRequest2 patchRequest =
                patch.PatchRequest as PatchRequest2;

            if (null == patchRequest)
            {
                string unsupportedPatchTypeName = patch.GetType().FullName;
                throw new NotSupportedException(unsupportedPatchTypeName);
            }

            if (this.storage.Groups.TryGetValue(patch.ResourceIdentifier.Identifier, out Core2Group group))
            {
                group.Apply(patchRequest);
                // Update metadata
                group.Metadata.LastModified = DateTime.UtcNow;
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return Task.CompletedTask;
        }
    }
}
