// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System.Runtime.Serialization;

    [DataContract]
    public abstract class UserBase : Resource
    {
        [DataMember(Name = AttributeNames.UserName)]
        public virtual string UserName
        {
            get;
            set;
        }
    }
}