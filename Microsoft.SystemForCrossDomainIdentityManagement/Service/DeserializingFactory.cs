//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System.Collections.Generic;

    public abstract class DeserializingFactory<TResource> :
            JsonDeserializingFactory<TResource>,
            IResourceJsonDeserializingFactory<TResource> where TResource : Resource, new()
    {
        public new TResource Create(IReadOnlyDictionary<string, object> json)
        {
            TResource result = base.Create(json);
            return result;
        }
    }
}