//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class BulkRequest2 : BulkOperations<BulkRequestOperation>
    {
        public BulkRequest2()
            : base(ProtocolSchemaIdentifiers.Version2BulkRequest)
        {
        }

        [DataMember(Name = ProtocolAttributeNames.FailOnErrors, Order = 1)]
        public int? FailOnErrors
        {
            get;
            set;
        }
    }
}
