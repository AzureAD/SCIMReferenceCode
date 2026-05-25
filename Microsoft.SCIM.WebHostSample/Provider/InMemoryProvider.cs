// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM.WebHostSample.Provider
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.SCIM;
    using Microsoft.SCIM.WebHostSample.Resources;

    public class InMemoryProvider : ProviderBase
    {
        private readonly ProviderBase groupProvider;
        private readonly ProviderBase userProvider;

        private static readonly Lazy<IReadOnlyCollection<TypeScheme>> TypeSchema =
            new Lazy<IReadOnlyCollection<TypeScheme>>(
                () =>
                    new TypeScheme[]
                    { 
                        SampleTypeScheme.UserTypeScheme,
                        SampleTypeScheme.GroupTypeScheme, 
                        SampleTypeScheme.EnterpriseUserTypeScheme,
                        SampleTypeScheme.ResourceTypesTypeScheme,
                        SampleTypeScheme.SchemaTypeScheme,
                        SampleTypeScheme.ServiceProviderConfigTypeScheme
                    });

        private static readonly Lazy<IReadOnlyCollection<Core2ResourceType>> Types =
            new Lazy<IReadOnlyCollection<Core2ResourceType>>(
                () =>
                    new Core2ResourceType[] { SampleResourceTypes.UserResourceType, SampleResourceTypes.GroupResourceType } );


        public InMemoryProvider()
        {
            this.groupProvider = new InMemoryGroupProvider();
            this.userProvider = new InMemoryUserProvider();
        }

        public override IReadOnlyCollection<Core2ResourceType> ResourceTypes => InMemoryProvider.Types.Value;
       
        public override IReadOnlyCollection<TypeScheme> Schema => InMemoryProvider.TypeSchema.Value;
        
        public override Task<Resource> CreateAsync(Resource resource, string correlationIdentifier)
        {
            if (resource is Core2EnterpriseUser)
            {
                return this.userProvider.CreateAsync(resource, correlationIdentifier);
            }

            if (resource is Core2Group)
            {
                return this.groupProvider.CreateAsync(resource, correlationIdentifier);
            }

            throw new NotImplementedException();
        }

        public override Task DeleteAsync(IResourceIdentifier resourceIdentifier, string correlationIdentifier)
        {
            if (resourceIdentifier.SchemaIdentifier.Equals(SchemaIdentifiers.Core2EnterpriseUser))
            {
                return this.userProvider.DeleteAsync(resourceIdentifier, correlationIdentifier);
            }

            if (resourceIdentifier.SchemaIdentifier.Equals(SchemaIdentifiers.Core2Group))
            {
                return this.groupProvider.DeleteAsync(resourceIdentifier, correlationIdentifier);
            }

            throw new  NotImplementedException();
        }

        public override Task<Resource[]> QueryAsync(IQueryParameters parameters, string correlationIdentifier)
        {
            if (parameters.SchemaIdentifier.Equals(SchemaIdentifiers.Core2EnterpriseUser))
            {
                return this.userProvider.QueryAsync(parameters, correlationIdentifier);
            }

            if (parameters.SchemaIdentifier.Equals(SchemaIdentifiers.Core2Group))
            {
                return this.groupProvider.QueryAsync(parameters, correlationIdentifier);
            }

            throw new NotImplementedException();
        }

        public override Task<Resource> ReplaceAsync(Resource resource, string correlationIdentifier)
        {
            if (resource is Core2EnterpriseUser)
            {
                return this.userProvider.ReplaceAsync(resource, correlationIdentifier);
            }

            if (resource is Core2Group)
            {
                return this.groupProvider.ReplaceAsync(resource, correlationIdentifier);
            }

            throw new NotImplementedException();
        }

        public override Task<Resource> RetrieveAsync(IResourceRetrievalParameters parameters, string correlationIdentifier)
        {
            if (parameters.SchemaIdentifier.Equals(SchemaIdentifiers.Core2EnterpriseUser))
            {
                return this.userProvider.RetrieveAsync(parameters, correlationIdentifier);
            }

            if (parameters.SchemaIdentifier.Equals(SchemaIdentifiers.Core2Group))
            {
                return this.groupProvider.RetrieveAsync(parameters, correlationIdentifier);
            }

            throw new NotImplementedException();
        }

        public override Task UpdateAsync(IPatch patch, string correlationIdentifier)
        {
            if (patch == null)
            {
                throw new ArgumentNullException(nameof(patch));
            }

            if (string.IsNullOrWhiteSpace(patch.ResourceIdentifier.Identifier))
            {
                throw new ArgumentException(nameof(patch));
            }

            if (string.IsNullOrWhiteSpace(patch.ResourceIdentifier.SchemaIdentifier))
            {
                throw new ArgumentException(nameof(patch));
            }

            if (patch.ResourceIdentifier.SchemaIdentifier.Equals(SchemaIdentifiers.Core2EnterpriseUser))
            {
                return this.userProvider.UpdateAsync(patch, correlationIdentifier);
            }

            if (patch.ResourceIdentifier.SchemaIdentifier.Equals(SchemaIdentifiers.Core2Group))
            {
                return this.groupProvider.UpdateAsync(patch, correlationIdentifier);
            }

            throw new NotImplementedException();
        }
    }
}
