using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.SCIM.WebHostSample.Resources
{
    public class SampleConstants
    {
        public const string Group = "Group";
        public const string GroupSchema = "urn:ietf:params:scim:schemas:core:2.0:Group";
        public const string User = "User";
        public const string UserAccount = "User Account";
        public const string UserCore2Schema = "urn:ietf:params:scim:schemas:core:2.0:User";
        public const string UserEnterprise = "Enterprise User";
        public const string UserEnterpriseName = "EnterpriseUser";
        public const string UserEnterpriseSchema = "urn:ietf:params:scim:schemas:extension:enterprise:2.0:User";
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



    }
}
