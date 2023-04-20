// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM.WebHostSample.Resources
{
    public class SampleSchemaAttributes
    {
        public static AttributeScheme NameAttributeScheme
        {
            get
            {
                AttributeScheme nameScheme = new AttributeScheme("name", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionSchemaName,
                    Mutability = Mutability.readOnly,
                    Required = true
                };
                return nameScheme;
            }
        }

        public static AttributeScheme DescriptionAttributeScheme
        {
            get
            {
                AttributeScheme descriptionScheme = new AttributeScheme("description", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionSchemaName,
                    Mutability = Mutability.readOnly,
                };
                return descriptionScheme;
            }
        }

        public static AttributeScheme AttributesAttributeScheme
        {
            get
            {
                AttributeScheme attributesScheme = new AttributeScheme("attributes", AttributeDataType.complex, true)
                {
                    Description = SampleConstants.DescriptionSchemaAttributes,
                    Mutability = Mutability.readOnly,
                };
                attributesScheme.AddSubAttribute(NameSubAttributeScheme);
                attributesScheme.AddSubAttribute(TypeSubAttributeScheme);
                attributesScheme.AddSubAttribute(MultiValuedSubAttributeScheme);
                attributesScheme.AddSubAttribute(DescriptionSubAttributeScheme);
                attributesScheme.AddSubAttribute(RequiredSubAttributeScheme);
                attributesScheme.AddSubAttribute(CanonicalValuesSubAttributeScheme);
                attributesScheme.AddSubAttribute(CaseExactSubAttributeScheme);
                attributesScheme.AddSubAttribute(MutabilitySubAttributeScheme);
                attributesScheme.AddSubAttribute(ReturnedSubAttributeScheme);
                attributesScheme.AddSubAttribute(UniquenessSubAttributeScheme);
                attributesScheme.AddSubAttribute(ReferenceTypesSubAttributeScheme);
                attributesScheme.AddSubAttribute(SubAttributesSubAttributeScheme);

                return attributesScheme;
            }
        }

        public static AttributeScheme NameSubAttributeScheme
        {
            get
            {
                AttributeScheme nameScheme = new AttributeScheme("name", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionAttributeName,
                    Mutability = Mutability.readOnly,
                    Required = true,
                    CaseExact = true
                };
                return nameScheme;
            }
        }

        public static AttributeScheme TypeSubAttributeScheme
        {
            get
            {
                AttributeScheme typeScheme = new AttributeScheme("type", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionAttributeType,
                    Mutability = Mutability.readOnly,
                    Required = true,
                };
                typeScheme.AddCanonicalValues("string");
                typeScheme.AddCanonicalValues("complex");
                typeScheme.AddCanonicalValues("boolean");
                typeScheme.AddCanonicalValues("decimal");
                typeScheme.AddCanonicalValues("integer");
                typeScheme.AddCanonicalValues("dateTime");
                typeScheme.AddCanonicalValues("reference");

                return typeScheme;
            }
        }
       public static AttributeScheme MultiValuedSubAttributeScheme
        {
            get
            {
                AttributeScheme multiValuedScheme = new AttributeScheme("multiValued", AttributeDataType.boolean, false)
                {
                    Description = SampleConstants.DescriptionAttributeMultiValued,
                    Mutability = Mutability.readOnly,
                    Required = true,
                };
                return multiValuedScheme;
            }
       }

        public static AttributeScheme DescriptionSubAttributeScheme
        {
            get
            {
                AttributeScheme descriptionScheme = new AttributeScheme("description", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionAttributeDescription,
                    Mutability = Mutability.readOnly,
                    CaseExact = true
                };
                return descriptionScheme;
            }
        }

        public static AttributeScheme RequiredSubAttributeScheme
        {
            get
            {
                AttributeScheme requiredScheme = new AttributeScheme("required", AttributeDataType.boolean, false)
                {
                    Description = SampleConstants.DescriptionAttributeRequired,
                    Mutability = Mutability.readOnly,
                };
                return requiredScheme;
            }
        }

        public static AttributeScheme CanonicalValuesSubAttributeScheme
        {
            get
            {
                AttributeScheme canonicalValuesScheme = new AttributeScheme("canonicalValues", AttributeDataType.@string, true)
                {
                    Description = SampleConstants.DescriptionAttributeCanonicalValues,
                    Mutability = Mutability.readOnly,
                    CaseExact = true
                };
                return canonicalValuesScheme;
            }
        }

        public static AttributeScheme CaseExactSubAttributeScheme
        {
            get
            {
                AttributeScheme caseExactScheme = new AttributeScheme("caseExact", AttributeDataType.boolean, false)
                {
                    Description = SampleConstants.DescriptionAttributeCaseExact,
                    Mutability = Mutability.readOnly,
                };
                return caseExactScheme;
            }
        }

        public static AttributeScheme MutabilitySubAttributeScheme
        {
            get
            {
                AttributeScheme mutabilityScheme = new AttributeScheme("mutability", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionAttributeMutability,
                    Mutability = Mutability.readOnly,
                    CaseExact = true
                };
                mutabilityScheme.AddCanonicalValues("readOnly");
                mutabilityScheme.AddCanonicalValues("readWrite");
                mutabilityScheme.AddCanonicalValues("immutable");
                mutabilityScheme.AddCanonicalValues("writeOnly");

                return mutabilityScheme;
            }
        }

        public static AttributeScheme ReturnedSubAttributeScheme
        {
            get
            {
                AttributeScheme returnedScheme = new AttributeScheme("returned", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionAttributeReturned,
                    Mutability = Mutability.readOnly,
                    CaseExact = true
                };
                returnedScheme.AddCanonicalValues("always");
                returnedScheme.AddCanonicalValues("never");
                returnedScheme.AddCanonicalValues("default");
                returnedScheme.AddCanonicalValues("request");

                return returnedScheme;
            }
        }

        public static AttributeScheme UniquenessSubAttributeScheme
        {
            get
            {
                AttributeScheme uniquenessScheme = new AttributeScheme("uniqueness", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionAttributeUniqueness,
                    Mutability = Mutability.readOnly,
                    CaseExact = true
                };
                uniquenessScheme.AddCanonicalValues("none");
                uniquenessScheme.AddCanonicalValues("server");
                uniquenessScheme.AddCanonicalValues("global");

                return uniquenessScheme;
            }
        }

        public static AttributeScheme ReferenceTypesSubAttributeScheme
        {
            get
            {
                AttributeScheme referenceTypesScheme = new AttributeScheme("referenceTypes", AttributeDataType.@string, true)
                {
                    Description = SampleConstants.DescriptionAttributeReferenceTypes,
                    Mutability = Mutability.readOnly,
                    CaseExact = true
                };

                return referenceTypesScheme;
            }
        }

        public static AttributeScheme SubAttributesSubAttributeScheme
        {
            get
            {
                AttributeScheme subAttributesScheme = new AttributeScheme("subAttributes", AttributeDataType.complex, true)
                {
                    Description = SampleConstants.DescriptionAttributeSubAttributes,
                    Mutability = Mutability.readOnly,
                };
                subAttributesScheme.AddSubAttribute(NameSubAttributeScheme);
                subAttributesScheme.AddSubAttribute(TypeSubAttributeScheme);
                subAttributesScheme.AddSubAttribute(MultiValuedSubAttributeScheme);
                subAttributesScheme.AddSubAttribute(DescriptionSubAttributeScheme);
                subAttributesScheme.AddSubAttribute(RequiredSubAttributeScheme);
                subAttributesScheme.AddSubAttribute(CanonicalValuesSubAttributeScheme);
                subAttributesScheme.AddSubAttribute(CaseExactSubAttributeScheme);
                subAttributesScheme.AddSubAttribute(MutabilitySubAttributeScheme);
                subAttributesScheme.AddSubAttribute(ReturnedSubAttributeScheme);
                subAttributesScheme.AddSubAttribute(UniquenessSubAttributeScheme);
                subAttributesScheme.AddSubAttribute(ReferenceTypesSubAttributeScheme);

                return subAttributesScheme;
            }
        }
    }
}
