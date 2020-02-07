//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System.Runtime.Serialization;

    [DataContract]
    public abstract class TypedItem
    {
        [DataMember(Name = AttributeNames.Type)]
        public string ItemType
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Primary, IsRequired = false)]
        public bool Primary
        {
            get;
            set;
        }
    }
}