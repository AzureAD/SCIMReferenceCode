// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;

    [DataContract]
    public abstract class Core2UserBase : UserBase
    {
        private IDictionary<string, IDictionary<string, object>> customExtension;

        protected Core2UserBase()
        {
            this.AddSchema(SchemaIdentifiers.Core2User);
            this.Metadata =
                new Core2Metadata()
                {
                    ResourceType = Types.User
                };
            this.OnInitialization();
        }

        [DataMember(Name = AttributeNames.Active)]
        public virtual bool Active
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Addresses, IsRequired = false, EmitDefaultValue = false)]
        public virtual IEnumerable<Address> Addresses
        {
            get;
            set;
        }

        public virtual IReadOnlyDictionary<string, IDictionary<string, object>> CustomExtension
        {
            get
            {
                return new ReadOnlyDictionary<string, IDictionary<string, object>>(this.customExtension);
            }
        }

        [DataMember(Name = AttributeNames.DisplayName, IsRequired = false, EmitDefaultValue = false)]
        public virtual string DisplayName
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.ElectronicMailAddresses, IsRequired = false, EmitDefaultValue = false)]
        public virtual IEnumerable<ElectronicMailAddress> ElectronicMailAddresses
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Ims, IsRequired = false, EmitDefaultValue = false)]
        public virtual IEnumerable<InstantMessaging> InstantMessagings
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Locale, IsRequired = false, EmitDefaultValue = false)]
        public virtual string Locale
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Metadata)]
        public virtual Core2Metadata Metadata
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Name, IsRequired = false, EmitDefaultValue = false)]
        public virtual Name Name
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Nickname, IsRequired = false, EmitDefaultValue = false)]
        public virtual string Nickname
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.PhoneNumbers, IsRequired = false, EmitDefaultValue = false)]
        public virtual IEnumerable<PhoneNumber> PhoneNumbers
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.PreferredLanguage, IsRequired = false, EmitDefaultValue = false)]
        public virtual string PreferredLanguage
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Roles, IsRequired = false, EmitDefaultValue = false)]
        public virtual IEnumerable<Role> Roles
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.TimeZone, IsRequired = false, EmitDefaultValue = false)]
        public virtual string TimeZone
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Title, IsRequired = false, EmitDefaultValue = false)]
        public virtual string Title
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.UserType, IsRequired = false, EmitDefaultValue = false)]
        public virtual string UserType
        {
            get;
            set;
        }

        public virtual void AddCustomAttribute(string key, object value)
        {
            if
            (
                    key != null
                && key.StartsWith(SchemaIdentifiers.PrefixExtension, StringComparison.OrdinalIgnoreCase)
                && !key.StartsWith(SchemaIdentifiers.Core2EnterpriseUser, StringComparison.OrdinalIgnoreCase)
                && value is Dictionary<string, object> nestedObject
            )
            {
                this.customExtension.Add(key, nestedObject);
            }
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            this.OnInitialization();
        }

        private void OnInitialization()
        {
            this.customExtension = new Dictionary<string, IDictionary<string, object>>();
        }

        public override Dictionary<string, object> ToJson()
        {
            Dictionary<string, object> result = base.ToJson();

            foreach (KeyValuePair<string, IDictionary<string, object>> entry in this.CustomExtension)
            {
                result.Add(entry.Key, entry.Value);
            }

            return result;
        }
    }
}