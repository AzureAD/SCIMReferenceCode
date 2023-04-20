// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;
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
