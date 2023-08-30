// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System.Runtime.Serialization;

    [DataContract]
    public abstract class RoleBase : TypedItem
    {
        [DataMember(Name = AttributeNames.Display, IsRequired = false, EmitDefaultValue = false)]
        public string Display
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Value, IsRequired = false, EmitDefaultValue = false)]
        public string Value
        {
            get;
            set;
        }
    }
}