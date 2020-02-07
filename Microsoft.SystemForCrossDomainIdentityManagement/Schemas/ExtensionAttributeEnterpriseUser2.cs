// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------


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