// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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