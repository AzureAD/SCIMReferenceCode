// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM.WebHostSample.Resources
{
    public static class SampleAddressesAttribute
    {
        public static AttributeScheme FormattedAddressAttributeScheme
        {
            get
            {
                AttributeScheme formattedAddressScheme = new AttributeScheme("formatted", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionFormattedAddress
                };
                return formattedAddressScheme;

            }
        }
        public static AttributeScheme StreetAddressAttributeScheme
        {
            get
            {
                AttributeScheme streetAddressScheme = new AttributeScheme("streetAddress", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionStreetAddress
                };
                return streetAddressScheme;

            }
        }

        public static AttributeScheme CountryAddressAttributeScheme
        {
            get
            {
                AttributeScheme countryAddressScheme = new AttributeScheme("country", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionAddressCountry
                };
                return countryAddressScheme;
            }
        }

        public static AttributeScheme LocalityAddressAttributeScheme
        {
            get
            {
                AttributeScheme localityAddressScheme = new AttributeScheme("locality", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionAddressLocality
                };
                return localityAddressScheme;
            }
        }

        public static AttributeScheme PostalCodeAddressAttributeScheme
        {
            get
            {
                AttributeScheme postalCodeAddressScheme = new AttributeScheme("postalCode", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionAddressPostalCode
                };
                return postalCodeAddressScheme;
            }
        }

        public static AttributeScheme RegionAddressAttributeScheme
        {
            get
            {
                AttributeScheme regionAddressScheme = new AttributeScheme("region", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionAddressRegion
                };
                return regionAddressScheme;
            }
        }
    }
}
