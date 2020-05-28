using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.SCIM.WebHostSample.Resources
{
    public static class SampleTypeScheme
    {

        public static TypeScheme UserTypeSceme
        {
            get
            {
                TypeScheme userType = new TypeScheme();
                userType.Description = SampleConstants.UserAccount;
                userType.Identifier = SampleConstants.UserCore2Schema;
                userType.Name = SampleConstants.User;
                userType.AddAttribute(UserNameAttributeScheme);
                userType.AddAttribute(NameAttributeScheme);
                userType.AddAttribute(DisplayNameAttributeScheme);
                userType.AddAttribute(TittleAttributeScheme);
                userType.AddAttribute(UserTypeAttributeScheme);
                userType.AddAttribute(PreferredLanguageAttrbiuteScheme);
                userType.AddAttribute(LocaleAttributeScheme);
                userType.AddAttribute(ActiveAttributeScheme);
                userType.AddAttribute(EmailsAttributeScheme);
                userType.AddAttribute(PhoneNumbersAttributeScheme);
                userType.AddAttribute(AddressesAttributeScheme);

                return userType;
            }
        }

        public static TypeScheme EnterpriseUserTypeScheme
        {
            get
            {
                TypeScheme enterpriseType = new TypeScheme();
                enterpriseType.Description = SampleConstants.UserEnterprise;
                enterpriseType.Identifier = SampleConstants.UserEnterpriseSchema;
                enterpriseType.Name = SampleConstants.UserEnterpriseName;
                enterpriseType.AddAttribute(ManagerAttributeScheme);
                enterpriseType.AddAttribute(EmployeeNumberAttributeScheme);
                enterpriseType.AddAttribute(CostcenterAttributeScheme);
                enterpriseType.AddAttribute(OrganizationAttributeScheme);
                enterpriseType.AddAttribute(DivisionAttributeScheme);
                enterpriseType.AddAttribute(DepartmentAttributeScheme);

                return enterpriseType;
            }
        }

        public static TypeScheme GroupTypeSceme
        {
            get
            {
                TypeScheme groupType = new TypeScheme();
                groupType.Description = SampleConstants.Group;
                groupType.Identifier = SampleConstants.GroupSchema;
                groupType.Name = SampleConstants.Group;
                groupType.AddAttribute(GroupDisplayNameAttributeScheme);
                groupType.AddAttribute(MembersAttributeScheme);

                return groupType;
            }
        }

        public static AttributeScheme GroupDisplayNameAttributeScheme
        {
            get
            {
                AttributeScheme groupDisplayScheme = new AttributeScheme("displayName", AttributeDataType.String, false);
                groupDisplayScheme.Description = SampleConstants.DescriptionGroupDisplayName;
                groupDisplayScheme.Required = true;
                groupDisplayScheme.Uniqueness = Uniqueness.server;

                return groupDisplayScheme;
            }
        }

        public static AttributeScheme MembersAttributeScheme
        {
            get
            {
                AttributeScheme membersScheme = new AttributeScheme("members", AttributeDataType.Complex, true);
                membersScheme.Description = SampleConstants.DescriptionMemebers;

                return membersScheme;
            }
        }

        public static AttributeScheme UserNameAttributeScheme
        {
            get
            {
                AttributeScheme userNameScheme = new AttributeScheme("userName", AttributeDataType.String, false);
                userNameScheme.Description = SampleConstants.DescriptionUserName;
                userNameScheme.Required = true;
                userNameScheme.Uniqueness = Uniqueness.server;
                return userNameScheme;
            }
        }

        public static AttributeScheme NameAttributeScheme
        {
            get
            {
                AttributeScheme nameScheme = new AttributeScheme("name", AttributeDataType.Complex, false);
                nameScheme.Description = SampleConstants.DescriptionName;
                return nameScheme;
            }
        }

        public static AttributeScheme DisplayNameAttributeScheme
        {
            get
            {
                AttributeScheme displayNameScheme = new AttributeScheme("displayName", AttributeDataType.String, false);
                displayNameScheme.Description = SampleConstants.DescriptionDisplayName;
                return displayNameScheme;
            }
        }

        public static AttributeScheme TittleAttributeScheme
        {
            get
            {
                AttributeScheme titleScheme = new AttributeScheme("title", AttributeDataType.String, false);
                titleScheme.Description = SampleConstants.DescriptionTitle;
                return titleScheme;
            }
        }

        public static AttributeScheme UserTypeAttributeScheme
        {
            get
            {
                AttributeScheme userTypeScheme = new AttributeScheme("userType", AttributeDataType.String, false);
                userTypeScheme.Description = SampleConstants.DescriptionUserType;
                return userTypeScheme;
            }
        }

        public static AttributeScheme PreferredLanguageAttrbiuteScheme
        {
            get
            {
                AttributeScheme preferredLanguageScheme = new AttributeScheme("preferredLanguage", AttributeDataType.String, false);
                preferredLanguageScheme.Description = SampleConstants.DescriptionPreferredLanguage;
                return preferredLanguageScheme;
            }
        }

        public static AttributeScheme LocaleAttributeScheme
        {
            get
            {
                AttributeScheme preferredLanguageScheme = new AttributeScheme("locale", AttributeDataType.String, false);
                preferredLanguageScheme.Description = SampleConstants.DescriptionLocale;
                return preferredLanguageScheme;
            }
        }

        public static AttributeScheme ActiveAttributeScheme
        {
            get
            {
                AttributeScheme activeScheme = new AttributeScheme("active", AttributeDataType.Boolean, false);
                activeScheme.Description = SampleConstants.DescriptionActive;
                return activeScheme;
            }
        }

        public static AttributeScheme EmailsAttributeScheme
        {
            get
            {
                AttributeScheme emailsScheme = new AttributeScheme("emails", AttributeDataType.Complex, true);
                emailsScheme.Description = SampleConstants.DescriptionEmails;
                return emailsScheme;
            }
        }

        public static AttributeScheme PhoneNumbersAttributeScheme
        {
            get
            {
                AttributeScheme phoneNumbersScheme = new AttributeScheme("phoneNumbers", AttributeDataType.Complex, true);
                phoneNumbersScheme.Description = SampleConstants.DescriptionPhoneNumbers;
                return phoneNumbersScheme;
            }
        }

        public static AttributeScheme AddressesAttributeScheme
        {
            get
            {
                AttributeScheme addressesScheme = new AttributeScheme("addresses", AttributeDataType.Complex, true);
                addressesScheme.Description = SampleConstants.DescriptionAddresses;
                return addressesScheme;
            }
        }

        public static AttributeScheme ManagerAttributeScheme
        {
            get
            {
                AttributeScheme managerScheme = new AttributeScheme("manager", AttributeDataType.Complex, false);
                managerScheme.Description = SampleConstants.DescriptionManager;
                return managerScheme;
            }
        }

        public static AttributeScheme EmployeeNumberAttributeScheme
        {
            get
            {
                AttributeScheme employeeNumberScheme = new AttributeScheme("employeeNumber", AttributeDataType.String, false);
                employeeNumberScheme.Description = SampleConstants.DescriptionEmployeeNumber;
                return employeeNumberScheme;
            }
        }

        public static AttributeScheme CostcenterAttributeScheme
        {
            get
            {
                AttributeScheme costCenterScheme = new AttributeScheme("costCenter", AttributeDataType.String, false);
                costCenterScheme.Description = SampleConstants.DescriptionCostCenter;
                return costCenterScheme;
            }
        }

        public static AttributeScheme OrganizationAttributeScheme
        {
            get
            {
                AttributeScheme organizationScheme = new AttributeScheme("organization", AttributeDataType.String, false);
                organizationScheme.Description = SampleConstants.DescriptionOrganization;
                return organizationScheme;
            }
        }

        public static AttributeScheme DivisionAttributeScheme
        {
            get
            {
                AttributeScheme divisionScheme = new AttributeScheme("division", AttributeDataType.String, false);
                divisionScheme.Description = SampleConstants.DescriptionDivision;
                return divisionScheme;
            }
        }

        public static AttributeScheme DepartmentAttributeScheme
        {
            get
            {
                AttributeScheme departmentScheme = new AttributeScheme("department", AttributeDataType.String, false);
                departmentScheme.Description = SampleConstants.Descriptiondepartment;
                return departmentScheme;
            }
        }
    }
}
