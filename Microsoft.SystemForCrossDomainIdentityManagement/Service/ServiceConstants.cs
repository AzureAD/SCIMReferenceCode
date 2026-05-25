// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    public static class ServiceConstants
    {
        public const string PathSegmentResourceTypes = "ResourceTypes";
        public const string PathSegmentSchemas = "Schemas";
        public const string PathSegmentServiceProviderConfiguration = "ServiceProviderConfig";
        public const string RouteGroups = SchemaConstants.PathInterface + ServiceConstants.SeparatorSegments + ProtocolConstants.PathGroups;
        public const string RouteResourceTypes = SchemaConstants.PathInterface + ServiceConstants.SeparatorSegments + ServiceConstants.PathSegmentResourceTypes;
        public const string RouteSchemas = SchemaConstants.PathInterface + ServiceConstants.SeparatorSegments + ServiceConstants.PathSegmentSchemas;
        public const string RouteServiceConfiguration = SchemaConstants.PathInterface + ServiceConstants.SeparatorSegments + ServiceConstants.PathSegmentServiceProviderConfiguration;
        public const string RouteUsers = SchemaConstants.PathInterface + ServiceConstants.SeparatorSegments + ProtocolConstants.PathUsers;
        public const string RouteBulk = SchemaConstants.PathInterface + ServiceConstants.SeparatorSegments + ProtocolConstants.PathBulk;
        public const string SeparatorSegments = "/";
    }
}
