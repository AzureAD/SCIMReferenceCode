// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
   using System.Runtime.Serialization;

    [DataContract]
    public sealed class BulkResponse2 : BulkOperations<BulkResponseOperation>
    {
        public BulkResponse2()
            : base(ProtocolSchemaIdentifiers.Version2BulkResponse)
        {
        }
    }
}
