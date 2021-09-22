// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM.WebHostSample.Resources
{
    public static class SampleUserAttributes
    {
        public static AttributeScheme UserNameAttributeScheme
        {
            get
            {
                AttributeScheme userNameScheme = new AttributeScheme("userName", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionUserName,
                    Required = true,
                    Uniqueness = Uniqueness.server
                };
                return userNameScheme;
            }
        }

        public static AttributeScheme NameAttributeScheme
        {
            get
            {
                AttributeScheme nameScheme = new AttributeScheme("name", AttributeDataType.complex, false)
                {
                    Description = SampleConstants.DescriptionName
                };
                nameScheme.AddSubAttribute(SampleNameAttribute.FormattedNameAttributeScheme);
                nameScheme.AddSubAttribute(SampleNameAttribute.GivenNameAttributeScheme);
                nameScheme.AddSubAttribute(SampleNameAttribute.FamilyNameAttributeScheme);
                nameScheme.AddSubAttribute(SampleNameAttribute.HonorificPrefixAttributeScheme);
                nameScheme.AddSubAttribute(SampleNameAttribute.HonorificSuffixAttributeScheme);

                return nameScheme;
            }
        }

        public static AttributeScheme DisplayNameAttributeScheme
        {
            get
            {
                AttributeScheme displayNameScheme = new AttributeScheme("displayName", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionDisplayName
                };
                return displayNameScheme;
            }
        }

        public static AttributeScheme NickNameAttributeScheme
        {
            get
            {
                AttributeScheme nickNameScheme = new AttributeScheme("nickName", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionNickName
                };
                return nickNameScheme;
            }
        }

        public static AttributeScheme TitleAttributeScheme
        {
            get
            {
                AttributeScheme titleScheme = new AttributeScheme("title", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionTitle
                };
                return titleScheme;
            }
        }

        public static AttributeScheme UserTypeAttributeScheme
        {
            get
            {
                AttributeScheme userTypeScheme = new AttributeScheme("userType", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionUserType
                };
                return userTypeScheme;
            }
        }

        public static AttributeScheme PreferredLanguageAttrbiuteScheme
        {
            get
            {
                AttributeScheme preferredLanguageScheme = new AttributeScheme("preferredLanguage", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionPreferredLanguage
                };
                return preferredLanguageScheme;
            }
        }

        public static AttributeScheme LocaleAttributeScheme
        {
            get
            {
                AttributeScheme localeScheme = new AttributeScheme("locale", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionLocale
                };
                return localeScheme;
            }
        }

        public static AttributeScheme TimezoneAttributeScheme
        {
            get
            {
                AttributeScheme timezoneScheme = new AttributeScheme("timezone", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionTimeZone
                };
                return timezoneScheme;
            }
        }

        public static AttributeScheme ActiveAttributeScheme
        {
            get
            {
                AttributeScheme activeScheme = new AttributeScheme("active", AttributeDataType.boolean, false)
                {
                    Description = SampleConstants.DescriptionActive
                };
                return activeScheme;
            }
        }

        public static AttributeScheme EmailsAttributeScheme
        {
            get
            {
                AttributeScheme emailsScheme = new AttributeScheme("emails", AttributeDataType.complex, true)
                {
                    Description = SampleConstants.DescriptionEmails
                };
                emailsScheme.AddSubAttribute(SampleMultivaluedAttributes.ValueSubAttributeScheme);
                emailsScheme.AddSubAttribute(SampleMultivaluedAttributes.Type2SubAttributeScheme);
                emailsScheme.AddSubAttribute(SampleMultivaluedAttributes.PrimarySubAttributeScheme);

                return emailsScheme;
            }
        }

        public static AttributeScheme PhoneNumbersAttributeScheme
        {
            get
            {
                AttributeScheme phoneNumbersScheme = new AttributeScheme("phoneNumbers", AttributeDataType.complex, true)
                {
                    Description = SampleConstants.DescriptionPhoneNumbers
                };
                phoneNumbersScheme.AddSubAttribute(SampleMultivaluedAttributes.ValueSubAttributeScheme);
                phoneNumbersScheme.AddSubAttribute(SampleMultivaluedAttributes.Type2SubAttributeScheme);
                phoneNumbersScheme.AddSubAttribute(SampleMultivaluedAttributes.PrimarySubAttributeScheme);

                return phoneNumbersScheme;
            }
        }

        public static AttributeScheme AddressesAttributeScheme
        {
            get
            {
                AttributeScheme addressesScheme = new AttributeScheme("addresses", AttributeDataType.complex, true)
                {
                    Description = SampleConstants.DescriptionAddresses
                };
                addressesScheme.AddSubAttribute(SampleAddressesAttribute.FormattedAddressAttributeScheme);
                addressesScheme.AddSubAttribute(SampleAddressesAttribute.StreetAddressAttributeScheme);
                addressesScheme.AddSubAttribute(SampleMultivaluedAttributes.Type2SubAttributeScheme);
                addressesScheme.AddSubAttribute(SampleMultivaluedAttributes.PrimarySubAttributeScheme);
                addressesScheme.AddSubAttribute(SampleAddressesAttribute.CountryAddressAttributeScheme);
                addressesScheme.AddSubAttribute(SampleAddressesAttribute.LocalityAddressAttributeScheme);
                addressesScheme.AddSubAttribute(SampleAddressesAttribute.PostalCodeAddressAttributeScheme);
                addressesScheme.AddSubAttribute(SampleAddressesAttribute.RegionAddressAttributeScheme);

                return addressesScheme;
            }
        }

        public static AttributeScheme ImsAttributeScheme
        {
            get
            {
                AttributeScheme imsScheme = new AttributeScheme("ims", AttributeDataType.complex, true)
                {
                    Description = SampleConstants.DescriptionIms
                };
                imsScheme.AddSubAttribute(SampleMultivaluedAttributes.ValueSubAttributeScheme);
                imsScheme.AddSubAttribute(SampleMultivaluedAttributes.TypeImsSubAttributeScheme);
                imsScheme.AddSubAttribute(SampleMultivaluedAttributes.PrimarySubAttributeScheme);

                return imsScheme;
            }
        }

        public static AttributeScheme RolessAttributeScheme
        {
            get
            {
                AttributeScheme rolesScheme = new AttributeScheme("roles", AttributeDataType.complex, true)
                {
                    Description = SampleConstants.DescriptionRoles
                };
                rolesScheme.AddSubAttribute(SampleMultivaluedAttributes.ValueSubAttributeScheme);
                rolesScheme.AddSubAttribute(SampleMultivaluedAttributes.DisplaySubAttributeScheme);
                rolesScheme.AddSubAttribute(SampleMultivaluedAttributes.TypeDefaultSubAttributeScheme);
                rolesScheme.AddSubAttribute(SampleMultivaluedAttributes.PrimarySubAttributeScheme);

                return rolesScheme;
            }
        }
    }
}
