// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM.WebHostSample.Resources
{
    public static class SampleServiceProviderConfigAttributes
    {
        public static AttributeScheme DocumentationUriAttributeScheme
        {
            get
            {
                AttributeScheme documentationUriScheme = new AttributeScheme("documentationUri", AttributeDataType.reference, false)
                {
                    Description = SampleConstants.DescriptionServiceProviderConfigDocumentationUri,
                    Mutability = Mutability.readOnly
                };
                documentationUriScheme.AddReferenceTypes("external");

                return documentationUriScheme;
            }
        }

        public static AttributeScheme PatchAttributeScheme
        {
            get
            {
                AttributeScheme patchScheme = new AttributeScheme("patch", AttributeDataType.complex, false)
                {
                    Description = SampleConstants.DescriptionServiceProviderConfigPatch,
                    Mutability = Mutability.readOnly,
                    Required = true
                };
                patchScheme.AddSubAttribute(SupportedSubAttributeScheme);

                return patchScheme;
            }
        }

        public static AttributeScheme BulkAttributeScheme
        {
            get
            {
                AttributeScheme bulkScheme = new AttributeScheme("bulk", AttributeDataType.complex, false)
                {
                    Description = SampleConstants.DescriptionServiceProviderConfigBulk,
                    Mutability = Mutability.readOnly,
                    Required = true
                };
                bulkScheme.AddSubAttribute(SupportedSubAttributeScheme);
                bulkScheme.AddSubAttribute(MaxOperationsSubAttributeScheme);
                bulkScheme.AddSubAttribute(MaxPayloadSizeSubAttributeScheme);

                return bulkScheme;
            }
        }

        public static AttributeScheme EtagAttributeScheme
        {
            get
            {
                AttributeScheme etagScheme = new AttributeScheme("etag", AttributeDataType.complex, false)
                {
                    Description = SampleConstants.DescriptionServiceProviderConfigEtag,
                    Mutability = Mutability.readOnly,
                    Required = true
                };
                etagScheme.AddSubAttribute(SupportedSubAttributeScheme);

                return etagScheme;
            }
        }

        public static AttributeScheme FilterAttributeScheme
        {
            get
            {
                AttributeScheme filterScheme = new AttributeScheme("filter", AttributeDataType.complex, false)
                {
                    Description = SampleConstants.DescriptionServiceProviderConfigFilter,
                    Mutability = Mutability.readOnly,
                    Required = true
                };
                filterScheme.AddSubAttribute(SupportedSubAttributeScheme);
                filterScheme.AddSubAttribute(MaxResultsSubAttributeScheme);

                return filterScheme;
            }
        }

        public static AttributeScheme ChangePasswordAttributeScheme
        {
            get
            {
                AttributeScheme changePasswordScheme = new AttributeScheme("changePassword", AttributeDataType.complex, false)
                {
                    Description = SampleConstants.DescriptionServiceProviderConfigChangePassword,
                    Mutability = Mutability.readOnly,
                    Required = true
                };
                changePasswordScheme.AddSubAttribute(SupportedSubAttributeScheme);

                return changePasswordScheme;
            }
        }

        public static AttributeScheme SortAttributeScheme
        {
            get
            {
                AttributeScheme sortScheme = new AttributeScheme("sort", AttributeDataType.complex, false)
                {
                    Description = SampleConstants.DescriptionServiceProviderConfigSort,
                    Mutability = Mutability.readOnly,
                    Required = true
                };
                sortScheme.AddSubAttribute(SupportedSubAttributeScheme);

                return sortScheme;
            }
        }

        public static AttributeScheme AuthenticationSchemesAttributeScheme
        {
            get
            {
                AttributeScheme authenticationSchemesScheme = new AttributeScheme("authenticationSchemes", AttributeDataType.complex, true)
                {
                    Description = SampleConstants.DescriptionServiceProviderAuthenticationSchemes,
                    Mutability = Mutability.readOnly,
                    Required = true
                };
                authenticationSchemesScheme.AddSubAttribute(SampleMultivaluedAttributes.TypeAuthenticationSchemesAttributeScheme);
                authenticationSchemesScheme.AddSubAttribute(NameSubAttributeScheme);
                authenticationSchemesScheme.AddSubAttribute(DescriptionSubAttributeScheme);
                authenticationSchemesScheme.AddSubAttribute(SpecUriSubAttributeScheme);
                authenticationSchemesScheme.AddSubAttribute(DocumentationUriSubAttributeScheme);

                return authenticationSchemesScheme;
            }
        }

        public static AttributeScheme SupportedSubAttributeScheme
        {
            get
            {
                AttributeScheme supportedScheme = new AttributeScheme("supported", AttributeDataType.boolean, false)
                {
                    Description = SampleConstants.DescriptionServiceProviderConfigSupported,
                    Mutability = Mutability.readOnly,
                    Required = true
                };
                return supportedScheme;
            }
        }

        public static AttributeScheme MaxOperationsSubAttributeScheme
        {
            get
            {
                AttributeScheme maxOperationsScheme = new AttributeScheme("maxOperations", AttributeDataType.integer, false)
                {
                    Description = SampleConstants.DescriptionServiceProviderConfigBulkMaxOperations,
                    Mutability = Mutability.readOnly,
                    Required = true
                };
                return maxOperationsScheme;
            }
        }

        public static AttributeScheme MaxPayloadSizeSubAttributeScheme
        {
            get
            {
                AttributeScheme maxPayloadSizeScheme = new AttributeScheme("maxPayloadSize", AttributeDataType.integer, false)
                {
                    Description = SampleConstants.DescriptionServiceProviderConfigBulkMaxPayloadSize,
                    Mutability = Mutability.readOnly,
                    Required = true
                };
                return maxPayloadSizeScheme;
            }
        }

        public static AttributeScheme MaxResultsSubAttributeScheme
        {
            get
            {
                AttributeScheme maxResultsScheme = new AttributeScheme("maxResults", AttributeDataType.integer, false)
                {
                    Description = SampleConstants.DescriptionServiceProviderConfigFilterMaxResults,
                    Mutability = Mutability.readOnly,
                    Required = true
                };
                return maxResultsScheme;
            }
        }

        public static AttributeScheme NameSubAttributeScheme
        {
            get
            {
                AttributeScheme nameScheme = new AttributeScheme("name", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionServiceProviderAuthenticationSchemesName,
                    Mutability = Mutability.readOnly,
                    Required = true
                };
                return nameScheme;
            }
        }

        public static AttributeScheme DescriptionSubAttributeScheme
        {
            get
            {
                AttributeScheme descriptionScheme = new AttributeScheme("description", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionServiceProviderAuthenticationSchemesDescription,
                    Mutability = Mutability.readOnly,
                    Required = true
                };
                return descriptionScheme;
            }
        }

        public static AttributeScheme SpecUriSubAttributeScheme
        {
            get
            {
                AttributeScheme specUriScheme = new AttributeScheme("specUri", AttributeDataType.reference, false)
                {
                    Description = SampleConstants.DescriptionServiceProviderAuthenticationSchemesSpecUri,
                    Mutability = Mutability.readOnly,
                };
                specUriScheme.AddReferenceTypes("external");

                return specUriScheme;
            }
        }

        public static AttributeScheme DocumentationUriSubAttributeScheme
        {
            get
            {
                AttributeScheme documentationUriScheme = new AttributeScheme("documentationUri", AttributeDataType.reference, false)
                {
                    Description = SampleConstants.DescriptionServiceProviderAuthenticationSchemesDocumentationUri,
                    Mutability = Mutability.readOnly,
                };
                documentationUriScheme.AddReferenceTypes("external");

                return documentationUriScheme;
            }
        }
    }
}
