// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM.WebHostSample.Resources
{
    public static class SampleResourceTypeAttributes
    {

        public static AttributeScheme NameAttributeScheme
        {
            get
            {
                AttributeScheme nameScheme = new AttributeScheme("name", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionResourceTypeName
                };
                return nameScheme;
            }
        }

        public static AttributeScheme EndpointAttributeScheme
        {
            get
            {
                AttributeScheme endpointScheme = new AttributeScheme("endpoint", AttributeDataType.reference, false)
                {
                    Description = SampleConstants.DescriptionResourceTypeEndpoint,
                    Required = true,
                    Mutability = Mutability.readOnly
                };
                endpointScheme.AddReferenceTypes("uri");

                return endpointScheme;
            }
        }

        public static AttributeScheme SchemaAttributeScheme
        {
            get
            {
                AttributeScheme schemaScheme = new AttributeScheme("schema", AttributeDataType.reference, false)
                {
                    Description = SampleConstants.DescriptionResourceTypeSchemaAttribute,
                    Required = true,
                    Mutability = Mutability.readOnly,
                    CaseExact = true
                };
                schemaScheme.AddReferenceTypes("uri");

                return schemaScheme;
            }
        }
    }
}
