//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class Name
    {
        [DataMember(Name = AttributeNames.Formatted, Order = 0, IsRequired = false, EmitDefaultValue = false)]
        public string Formatted
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.FamilyName, Order = 1, IsRequired = false, EmitDefaultValue = false)]
        public string FamilyName
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.GivenName, Order = 1, IsRequired = false, EmitDefaultValue = false)]
        public string GivenName
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.HonorificPrefix, Order = 1, IsRequired = false, EmitDefaultValue = false)]
        public string HonorificPrefix
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.HonorificSuffix, Order = 1, IsRequired = false, EmitDefaultValue = false)]
        public string HonorificSuffix
        {
            get;
            set;
        }
    }
}