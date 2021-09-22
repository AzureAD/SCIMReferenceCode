//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

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
