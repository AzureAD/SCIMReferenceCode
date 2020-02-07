//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public abstract class GroupBase : Resource
    {
        [DataMember(Name = AttributeNames.DisplayName)]
        public virtual string DisplayName
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Members, IsRequired = false, EmitDefaultValue = false)]
        public virtual IEnumerable<Member> Members
        {
            get;
            set;
        }
    }
}