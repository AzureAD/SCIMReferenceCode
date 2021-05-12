// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM.WebHostSample.Resources
{
    public static class SampleEnterpriseUserAttributes
    {
        public static AttributeScheme ManagerAttributeScheme
        {
            get
            {
                AttributeScheme managerScheme = new AttributeScheme("manager", AttributeDataType.complex, false)
                {
                    Description = SampleConstants.DescriptionManager
                };
                managerScheme.AddSubAttribute(SampleMultivaluedAttributes.ValueSubAttributeScheme);

                return managerScheme;
            }
        }

        public static AttributeScheme EmployeeNumberAttributeScheme
        {
            get
            {
                AttributeScheme employeeNumberScheme = new AttributeScheme("employeeNumber", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionEmployeeNumber
                };
                return employeeNumberScheme;
            }
        }

        public static AttributeScheme CostcenterAttributeScheme
        {
            get
            {
                AttributeScheme costCenterScheme = new AttributeScheme("costCenter", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionCostCenter
                };
                return costCenterScheme;
            }
        }

        public static AttributeScheme OrganizationAttributeScheme
        {
            get
            {
                AttributeScheme organizationScheme = new AttributeScheme("organization", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionOrganization
                };
                return organizationScheme;
            }
        }

        public static AttributeScheme DivisionAttributeScheme
        {
            get
            {
                AttributeScheme divisionScheme = new AttributeScheme("division", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionDivision
                };
                return divisionScheme;
            }
        }

        public static AttributeScheme DepartmentAttributeScheme
        {
            get
            {
                AttributeScheme departmentScheme = new AttributeScheme("department", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.Descriptiondepartment
                };
                return departmentScheme;
            }
        }
    }
}
