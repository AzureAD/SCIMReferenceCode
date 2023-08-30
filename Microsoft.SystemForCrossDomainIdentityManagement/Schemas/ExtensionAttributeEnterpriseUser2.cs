// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class ExtensionAttributeEnterpriseUser2 : ExtensionAttributeEnterpriseUserBase
    {
        [DataMember(Name = AttributeNames.Manager, IsRequired = false, EmitDefaultValue = false)]
        public Manager Manager
        {
            get;
            set;
        }
    }
}