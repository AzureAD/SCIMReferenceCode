//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    public static class ServiceConstants
    {
        public const string PathSegmentResourceTypes = "ResourceTypes";
        public const string PathSegmentToken = "token";
        public const string PathSegmentSchemas = "Schemas";
        public const string PathSegmentServiceProviderConfiguration = "ServiceProviderConfig";

        public const string RouteGroups = SchemaConstants.PathInterface + ServiceConstants.SeparatorSegments + ProtocolConstants.PathGroups;
        public const string RouteResourceTypes = SchemaConstants.PathInterface + ServiceConstants.SeparatorSegments + ServiceConstants.PathSegmentResourceTypes;
        public const string RouteSchemas = SchemaConstants.PathInterface + ServiceConstants.SeparatorSegments + ServiceConstants.PathSegmentSchemas;
        public const string RouteServiceConfiguration = SchemaConstants.PathInterface + ServiceConstants.SeparatorSegments + ServiceConstants.PathSegmentServiceProviderConfiguration;
        public const string RouteToken = SchemaConstants.PathInterface + ServiceConstants.SeparatorSegments + ServiceConstants.PathSegmentToken;
        public const string RouteUsers = SchemaConstants.PathInterface + ServiceConstants.SeparatorSegments + ProtocolConstants.PathUsers;

        public const string SeparatorSegments = "/";

        internal const string TokenAudience = "Microsoft.Security.Bearer";
        internal const string TokenIssuer = "Microsoft.Security.Bearer";
    }
}