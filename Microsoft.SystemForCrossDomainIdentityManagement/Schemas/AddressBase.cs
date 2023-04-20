// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System.Runtime.Serialization;

    [DataContract]
    public abstract class AddressBase : TypedItem
    {
        public const string Home = "home";
        public const string Other = "other";
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Untyped", Justification = "False analysis")]
        public const string Untyped = "untyped";
        public const string Work = "work";

        internal AddressBase()
        {
        }

        [DataMember(Name = AttributeNames.Country, IsRequired = false, EmitDefaultValue = false)]
        public string Country
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Formatted, IsRequired = false, EmitDefaultValue = false)]
        public string Formatted
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Locality, IsRequired = false, EmitDefaultValue = false)]
        public string Locality
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.PostalCode, IsRequired = false, EmitDefaultValue = false)]
        public string PostalCode
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Region, IsRequired = false, EmitDefaultValue = false)]
        public string Region
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.StreetAddress, IsRequired = false, EmitDefaultValue = false)]
        public string StreetAddress
        {
            get;
            set;
        }
    }
}