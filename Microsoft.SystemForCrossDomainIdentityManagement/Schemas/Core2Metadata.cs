//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
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
        [DataMember(Name = AttributeNames.Created, Order = 1)]
        public DateTime Created
        {
            get;
            set;
        }
        [DataMember(Name = AttributeNames.LastModified, Order = 2)]
        public DateTime LastModified
        {
            get;
            set;
        }
        [DataMember(Name = AttributeNames.Version, Order = 3)]
        public string Version
        {
            get;
            set;
        }
        [DataMember(Name = AttributeNames.Location, Order = 4)]
        public string Location
        {
            get;
            set;
        }
    }
}