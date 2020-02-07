//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class AttributeScheme
    {
        private AttributeDataType dataType;
        private string dataTypeValue;
        private Mutability mutability;
        private string mutabilityValue;
        private Returned returned;
        private string returnedValue;
        private Uniqueness uniqueness;
        private string uniquenessValue;

        public AttributeScheme()
        {
        }

        public AttributeScheme(string name, AttributeDataType type, bool plural)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            this.Name = name;
            this.DataType = type;
            this.Plural = plural;
            this.Mutability = Mutability.readWrite;
            this.Returned = Returned.@default;
            this.Uniqueness = Uniqueness.none;
        }

        [DataMember(Name = AttributeNames.CaseExact)]
        public bool CaseExact
        {
            get;
            set;
        }

        public AttributeDataType DataType
        {
            get
            {
                return this.dataType;
            }

            set
            {
                this.dataTypeValue = Enum.GetName(typeof(AttributeDataType), value);
                this.dataType = value;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called upon serialization")]
        [DataMember(Name = AttributeNames.Type)]
#pragma warning disable IDE0051 // Remove unused private members
        private string DataTypeValue
#pragma warning restore IDE0051 // Remove unused private members
        {
            get
            {
                return this.dataTypeValue;
            }

            set
            {
                this.dataType = (AttributeDataType)Enum.Parse(typeof(AttributeDataType), value);
                this.dataTypeValue = value;
            }
        }

        [DataMember(Name = AttributeNames.Description)]
        public string Description
        {
            get;
            set;
        }

        public Mutability Mutability
        {
            get
            {
                return this.mutability;
            }

            set
            {
                this.mutabilityValue = Enum.GetName(typeof(Mutability), value);
                this.mutability = value;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called upon serialization")]
        [DataMember(Name = AttributeNames.Mutability)]
#pragma warning disable IDE0051 // Remove unused private members
        private string MutabilityValue
#pragma warning restore IDE0051 // Remove unused private members
        {
            get
            {
                return this.mutabilityValue;
            }

            set
            {
                this.mutability = (Mutability)Enum.Parse(typeof(Mutability), value);
                this.mutabilityValue = value;
            }
        }

        [DataMember(Name = AttributeNames.Name)]
        public string Name
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Plural)]
        public bool Plural
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Required)]
        public bool Required
        {
            get;
            set;
        }

        public Returned Returned
        {
            get
            {
                return this.returned;
            }

            set
            {
                this.returnedValue = Enum.GetName(typeof(Returned), value);
                this.returned = value;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called upon serialization")]
        [DataMember(Name = AttributeNames.Returned)]
#pragma warning disable IDE0051 // Remove unused private members
        private string ReturnedValue
#pragma warning restore IDE0051 // Remove unused private members
        {
            get
            {
                return this.returnedValue;
            }

            set
            {
                this.returned = (Returned)Enum.Parse(typeof(Returned), value);
                this.returnedValue = value;
            }
        }

        public Uniqueness Uniqueness
        {
            get
            {
                return this.uniqueness;
            }

            set
            {
                this.uniquenessValue = Enum.GetName(typeof(Uniqueness), value);
                this.uniqueness = value;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called upon serialization")]
        [DataMember(Name = AttributeNames.Uniqueness)]
#pragma warning disable IDE0051 // Remove unused private members
        private string UniquenessValue
#pragma warning restore IDE0051 // Remove unused private members
        {
            get
            {
                return this.uniquenessValue;
            }

            set
            {
                this.uniqueness = (Uniqueness)Enum.Parse(typeof(Uniqueness), value);
                this.uniquenessValue = value;
            }
        }
    }
}