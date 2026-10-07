//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class PatchRequest2 : PatchRequest2Base<PatchOperation2Combined>
    {
        public PatchRequest2()
        {
        }

        public PatchRequest2(IReadOnlyCollection<PatchOperation2Combined> operations)
            : base(operations)
        {
        }
    }
}
