//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    public static class ProtocolSchemaIdentifiers
    {
        private const string Error = "Error";

        private const string OperationPatch = "PatchOp";

        private const string VersionMessages2 = "2.0:";

        private const string PrefixMessages = "urn:ietf:params:scim:api:messages:";
        public const string PrefixMessages2 = ProtocolSchemaIdentifiers.PrefixMessages + ProtocolSchemaIdentifiers.VersionMessages2;

        private const string ResponseList = "ListResponse";
        private const string RequestBulk = "BulkRequest";

        private const string ResponseBulk = "BulkResponse";

        public const string Version2Error =
            ProtocolSchemaIdentifiers.PrefixMessages2 + ProtocolSchemaIdentifiers.Error;

        public const string Version2ListResponse =
            ProtocolSchemaIdentifiers.PrefixMessages2 + ProtocolSchemaIdentifiers.ResponseList;

        public const string Version2PatchOperation =
            ProtocolSchemaIdentifiers.PrefixMessages2 + ProtocolSchemaIdentifiers.OperationPatch;
        public const string Version2BulkRequest =
             ProtocolSchemaIdentifiers.PrefixMessages2 + ProtocolSchemaIdentifiers.RequestBulk;

        public const string Version2BulkResponse =
            ProtocolSchemaIdentifiers.PrefixMessages2 + ProtocolSchemaIdentifiers.ResponseBulk;
    }
}
