// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM.WebHostSample.Resources
{
    public static class SampleNameAttribute
    {
        public static AttributeScheme FormattedNameAttributeScheme
        {
            get
            {
                AttributeScheme formattedNameScheme = new AttributeScheme("formatted", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionFormattedName
                };
                return formattedNameScheme;
            }
        }

        public static AttributeScheme GivenNameAttributeScheme
        {
            get
            {
                AttributeScheme givenNameScheme = new AttributeScheme("givenName", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionGivenName
                };
                return givenNameScheme;
            }
        }

        public static AttributeScheme FamilyNameAttributeScheme
        {
            get
            {
                AttributeScheme familyNameScheme = new AttributeScheme("familyName", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionFamilyName
                };
                return familyNameScheme;
            }
        }

        public static AttributeScheme HonorificPrefixAttributeScheme
        {
            get
            {
                AttributeScheme honorificPrefixScheme = new AttributeScheme("honorificPrefix", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionHonorificPrefix
                };
                return honorificPrefixScheme;
            }
        }

        public static AttributeScheme HonorificSuffixAttributeScheme
        {
            get
            {
                AttributeScheme honorificSuffixScheme = new AttributeScheme("honorificSuffix", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionHonorificSuffix
                };
                return honorificSuffixScheme;
            }
        }
    }
}
