// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System.Runtime.Serialization;

    [DataContract]
    public abstract class MemberBase
    {
        internal MemberBase()
        {
        }

        [DataMember(Name = AttributeNames.Type, IsRequired = false, EmitDefaultValue = false)]
        public string TypeName
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Value)]
        public string Value
        {
            get;
            set;
        }
    }
}