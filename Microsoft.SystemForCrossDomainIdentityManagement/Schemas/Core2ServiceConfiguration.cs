// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class Core2ServiceConfiguration : ServiceConfigurationBase
    {
        public Core2ServiceConfiguration(
            BulkRequestsFeature bulkRequestsSupport,
            bool supportsEntityTags,
            bool supportsFiltering,
            bool supportsPasswordChange,
            bool supportsPatching,
            bool supportsSorting)
        {
            this.AddSchema(SchemaIdentifiers.Core2ServiceConfiguration);
            this.Metadata =
                new Core2Metadata()
                {
                    ResourceType = Types.ServiceProviderConfiguration
                };

            this.BulkRequests = bulkRequestsSupport;
            this.EntityTags = new Feature(supportsEntityTags);
            this.Filtering = new Feature(supportsFiltering);
            this.PasswordChange = new Feature(supportsPasswordChange);
            this.Patching = new Feature(supportsPatching);
            this.Sorting = new Feature(supportsSorting);
        }

        [DataMember(Name = AttributeNames.Metadata)]
        public Core2Metadata Metadata
        {
            get;
            set;
        }
    }
}