//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class Core2Metadata
    {
        [DataMember(Name = AttributeNames.ResourceType, Order = 0)]
        public string ResourceType
        {
            get;
            set;
        }
    }
}