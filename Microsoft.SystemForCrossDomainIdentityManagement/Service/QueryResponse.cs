//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class QueryResponse : QueryResponseBase
    {
        public QueryResponse()
            : base()
        {
        }

        public QueryResponse(IReadOnlyCollection<Resource> resources)
            : base(resources)
        {
        }

        public QueryResponse(IList<Resource> resources)
            : base(resources)
        {
        }
    }
}