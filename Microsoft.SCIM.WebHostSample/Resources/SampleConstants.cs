// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM.WebHostSample.Resources
{
    public class SampleConstants
    {
        public const string Core2SchemaPrefix = "urn:ietf:params:scim:schemas:core:2.0:";
        public const string UserAccount = "User Account";
        public const string ServiceProviderConfig = "Service Provider Configuration";
        public const string UserEnterprise = "Enterprise User";
        public const string UserEnterpriseName = "EnterpriseUser";
        public const string UserEnterpriseSchema = "urn:ietf:params:scim:schemas:extension:enterprise:2.0:User";
        public const string DescriptionResourceTypeSchema = "Specifies the schema that describes a SCIM resource type";
        public const string DescriptionActive = "A Boolean value indicating the User's administrative status.";
        public const string DescriptionAddresses = "A physical mailing address for this User. Canonical type" +
            " values of \"work\", \"home\", and \"other\".  This attribute is a complex type with the following sub-attributes.";
        public const string DescriptionCostCenter = "Identifies the name of a cost center.";
        public const string DescriptionDisplayName = "The name of the User, suitable for display to end-users.  " +
            "The name SHOULD be the full name of the User being described, if known.";
        public const string DescriptionDivision = "Identifies the name of a division.";
        public const string Descriptiondepartment = "Identifies the name of a department.";
        public const string DescriptionEmails = "Email addresses for the user.  The value SHOULD be canonicalized by " +
            "the service provider, e.g., \"bjensen@example.com\" instead of \"bjensen@EXAMPLE.COM\". Canonical type values " +
            "of \"work\", \"home\", and \"other\".";
        public const string DescriptionEmployeeNumber = "Numeric or alphanumeric identifier assigned to a person, " +
            "typically based on order of hire or association with an organization.";
        public const string DescriptionGroupDisplayName = "A human-readable name for the Group. REQUIRED.";
        public const string DescriptionLocale = "Used to indicate the User's default location for purposes of localizing" +
            " items such as currency, date time format, or numerical representations.";
        public const string DescriptionManager = "The User's manager.  A complex type that optionally allows " +
            "service providers to represent organizational hierarchy by referencing the \"id\" attribute of another User.";
        public const string DescriptionMemebers = "A list of members of the Group.";
        public const string DescriptionName = "The components of the user's real name. Providers MAY return just " +
            "the full name as a single string in the formatted sub-attribute, or they MAY return just the " +
            "individual component attributes using the other sub-attributes, or they MAY return both.  " +
            "If both variants are returned, they SHOULD be describing the same name, with the formatted name " +
            "indicating how the component attributes should be combined.";
        public const string DescriptionOrganization = "Identifies the name of an organization.";
        public const string DescriptionPhoneNumbers = "Phone numbers for the User.  The value SHOULD be canonicalized" +
            " by the service provider according to the format specified in RFC 3966, e.g., \"tel:+1-201-555-0123\". " +
            "Canonical type values of \"work\", \"home\", \"mobile\", \"fax\", \"pager\", and \"other\".";
        public const string DescriptionPreferredLanguage = "Indicates the User's preferred written or spoken language.  " +
            "Generally used for selecting a localized user interface; e.g., \"en_US\" specifies the language English and country US.";
        public const string DescriptionTitle = "The user's title, such as \"Vice President.\"";
        public const string DescriptionUserName = "Unique identifier for the User, typically used by the user to " +
            "directly authenticate to the service provider. Each User MUST include a non-empty userName value.  " +
            "This identifier MUST be unique across the service provider's entire set of Users. REQUIRED.";
        public const string DescriptionUserType = "Used to identify the relationship between the organization and the user.  " +
            "Typical values used might be \"Contractor\", \"Employee\", \"Intern\", \"Temp\", \"External\", and \"Unknown\", " +
            "but any value may be used.";
        public const string DescriptionValue = "The significant value for the attribute";
        public const string DescriptionType = "A label indicating the attribute's function";
        public const string DescriptionFormattedName = "The full name, including all middle " +
            "names, titles, and suffixes as appropriate, formatted for display e.g., 'Ms. Barbara J Jensen, III').";
        public const string DescriptionGivenName = "The given name of the User, or" +
            " first name in most Western languages(e.g., 'Barbara' given the full name 'Ms. Barbara J Jensen, III').";
        public const string DescriptionDisplay = "A human-readable name, primarily used for display purposes.READ-ONLY.";
        public const string DescriptionPrimary = "A Boolean value indicating the 'primary' " + " or preferred attribute value for this attribute, " +
            "The primary attribute value 'true' MUST appear no more than once.";
        public const string DescriptionStreetAddress = "The full street address component, which may include house number, street name, P.O.box, and multi-line" +
            "extended street address information.This attribute MAY contain newlines.";
        public const string DescriptionFormattedAddress = "The full mailing address, formatted for display or use with a mailing label." +
            " This attribute MAY contain newlines.";
        public const string DescriptionFamilyName = "The family name of the User, or last name in most Western languages(e.g., 'Jensen' given the full " +
            "name 'Ms. Barbara J Jensen, III').";
        public const string DescriptionHonorificPrefix = "The honorific prefix(es) of the User, or title in most Western languages(e.g., 'Ms.' " +
            "given the full name 'Ms. Barbara J Jensen, III').";
        public const string DescriptionHonorificSuffix = "The honorific suffix(es) of the User, or suffix in most Western languages(e.g., 'III' " +
            "given the full name 'Ms. Barbara J Jensen, III').";
        public const string DescriptionNickName = "The casual way to address the user in real life, e.g., 'Bob' or 'Bobby' instead of 'Robert'. " +
            "This attribute SHOULD NOT be used to represent a User's username (e.g., 'bjensen' or 'mpepperidge').";
        public const string DescriptionTimeZone = "The User's time zone in the 'Olson' time zone database format, e.g., 'America/Los_Angeles'.";
        public const string DescriptionPassword = "The User's cleartext password.  This attribute is intended to be used as a means to specify an initial " +
            "password when creating a new User or to reset an existing User's password.";
        public const string DescriptionIms = "Instant messaging addresses for the User.";
        public const string DescriptionRoles = "A list of roles for the User that collectively represent who the User is, e.g., 'Student', 'Faculty'.";
        public const string DescriptionIdentifier = "The server unique id of a SCIM resource";
        public const string DescriptionResourceTypeName = "The resource type name.  When applicable,service providers MUST specify the name, e.g., 'User'.";
        public const string DescriptionResourceTypeEndpoint = "The resource type's HTTP-addressable endpoint relative to the Base URL, e.g., '/Users'.";
        public const string DescriptionResourceTypeSchemaAttribute = "The resource type's primary/base schema URI.";
        public const string DescriptionScimSchema = "Specifies the schema that describes a SCIM schema";
        public const string DescriptionSchemaName = "The schema's human-readable name.  When applicable, service providers MUST specify the name, e.g., 'User'.";
        public const string DescriptionSchemaAttributes = "A complex attribute that includes the attributes of a schema.";
        public const string DescriptionAttributeName = "The attribute's name.";
        public const string DescriptionAttributeType = "The attribute's data type. Valid values include 'string', 'complex', 'boolean', 'decimal', 'integer', 'dateTime', 'reference'.";
        public const string DescriptionAttributeMultiValued = "A Boolean value indicating an attribute's plurality.";
        public const string DescriptionAttributeDescription = "A human-readable description of the attribute.";
        public const string DescriptionAttributeRequired = "A boolean value indicating whether or not the attribute is required.";
        public const string DescriptionAttributeCanonicalValues = "A collection of canonical values.  When applicable, service providers MUST specify the canonical types, e.g., 'work', 'home'.";
        public const string DescriptionAttributeCaseExact = "A Boolean value indicating whether or not a string attribute is case sensitive.";
        public const string DescriptionAttributeMutability = "Indicates whether or not an attribute is modifiable.";
        public const string DescriptionAttributeReturned = "Indicates when an attribute is returned in a response(e.g., to a query).";
        public const string DescriptionAttributeUniqueness = "Indicates how unique a value must be.";
        public const string DescriptionAttributeReferenceTypes = "Used only with an attribute of type 'reference'.  Specifies a SCIM resourceType that a reference attribute MAY refer to, e.g., 'User'.";
        public const string DescriptionAttributeSubAttributes = "Used to define the sub-attributes of a complex attribute.";
        public const string DescriptionServiceProviderConfigSchema = "Schema for representing the service provider's configuration";
        public const string DescriptionServiceProviderConfigDocumentationUri = "An HTTP-addressable URL pointing to the service provider's human-consumable help documentation.";
        public const string DescriptionServiceProviderConfigPatch = "A complex type that specifies PATCH configuration options.";
        public const string DescriptionServiceProviderConfigSupported = "A Boolean value specifying whether or not the operation is supported.";
        public const string DescriptionServiceProviderConfigBulk = "A complex type that specifies bulk configuration options.";
        public const string DescriptionServiceProviderConfigEtag = "A complex type that specifies ETag configuration options.REQUIRED.";
        public const string DescriptionServiceProviderConfigBulkMaxOperations = "An integer value specifying the maximum number of operations.";
        public const string DescriptionServiceProviderConfigBulkMaxPayloadSize = "An integer value specifying the maximum payload size in bytes.";
        public const string DescriptionServiceProviderConfigFilter = "A complex type that specifies FILTER options.";
        public const string DescriptionServiceProviderConfigFilterMaxResults = "An integer value specifying the maximum number of resources returned in a response.";
        public const string DescriptionServiceProviderConfigChangePassword = "A complex type that specifies configuration options related to changing a password.";
        public const string DescriptionServiceProviderConfigSort = "A complex type that specifies sort result options.";
        public const string DescriptionServiceProviderAuthenticationSchemes = "A complex type that specifies supported authentication scheme properties.";
        public const string DescriptionServiceProviderAuthenticationSchemesName = "The common authentication scheme name, e.g., HTTP Basic.";
        public const string DescriptionServiceProviderAuthenticationSchemesDescription = "A description of the authentication scheme.";
        public const string DescriptionServiceProviderAuthenticationSchemesSpecUri = "An HTTP-addressable URL pointing to the authentication scheme's specification.";
        public const string DescriptionServiceProviderAuthenticationSchemesDocumentationUri = "An HTTP-addressable URL pointing to the authentication scheme's usage documentation.";
        public const string DescriptionAddressCountry = "The country name component.";
        public const string DescriptionAddressLocality = "The city or locality component.";
        public const string DescriptionAddressPostalCode = "The country name component.";
        public const string DescriptionAddressRegion = "The state or region component.";
        public const string DescriptionAddressType = "A label indicating the attribute's function, e.g., 'work' or 'home'.";
        public const  string SampleScimEndpoint = "http://localhost:5000/scim";
    }
}
