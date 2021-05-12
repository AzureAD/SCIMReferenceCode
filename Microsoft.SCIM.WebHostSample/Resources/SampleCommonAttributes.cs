// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM.WebHostSample.Resources
{
    public static class SampleCommonAttributes
    {
        public static AttributeScheme IdentiFierAttributeScheme
        {
            get
            {
                AttributeScheme idScheme = new AttributeScheme("id", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionIdentifier
                };
                return idScheme;
            }
        }
    }
}
