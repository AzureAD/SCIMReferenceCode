// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;
    using System.Configuration;

    internal class SystemForCrossDomainIdentityManagementConfigurationSection :
        ConfigurationSection, ISystemForCrossDomainIdentityManagementConfiguration
    {
        private const string DefaultValueAcceptLargeObjects = "false";

        private const string NameSection = "scimProtocol";

        private const string PropertyNameAcceptLargeObjects = "acceptLargeObjects";

        private static readonly bool DefaultAcceptLargeObjects =
            bool.Parse(SystemForCrossDomainIdentityManagementConfigurationSection.DefaultValueAcceptLargeObjects);

        private static readonly Lazy<SystemForCrossDomainIdentityManagementConfigurationSection> DefaultConfiguration =
            new Lazy<SystemForCrossDomainIdentityManagementConfigurationSection>(
                () =>
                    new SystemForCrossDomainIdentityManagementConfigurationSection());

        private static readonly Lazy<ConfigurationFactory<SystemForCrossDomainIdentityManagementConfigurationSection, ConfigurationErrorsException>> Factory =
            new Lazy<ConfigurationFactory<SystemForCrossDomainIdentityManagementConfigurationSection, ConfigurationErrorsException>>(
                () =>
                    new ConfigurationSectionFactory<SystemForCrossDomainIdentityManagementConfigurationSection>(
                        SystemForCrossDomainIdentityManagementConfigurationSection.NameSection));

        private static readonly Lazy<ISystemForCrossDomainIdentityManagementConfiguration> Singleton =
            new Lazy<ISystemForCrossDomainIdentityManagementConfiguration>(
                () =>
                    SystemForCrossDomainIdentityManagementConfigurationSection.Initialize());

        private SystemForCrossDomainIdentityManagementConfigurationSection()
        {
        }

        public static ISystemForCrossDomainIdentityManagementConfiguration Instance
        {
            get
            {
                return SystemForCrossDomainIdentityManagementConfigurationSection.Singleton.Value;
            }
        }

        public bool AcceptLargeObjects
        {
            get
            {
                if (!bool.TryParse(this.AcceptLargeObjectsValue, out bool result))
                {
                    result = SystemForCrossDomainIdentityManagementConfigurationSection.DefaultAcceptLargeObjects;
                }

                return result;
            }
        }

        [ConfigurationProperty(
            SystemForCrossDomainIdentityManagementConfigurationSection.PropertyNameAcceptLargeObjects,
            DefaultValue = SystemForCrossDomainIdentityManagementConfigurationSection.DefaultValueAcceptLargeObjects,
            IsRequired = false)]
        private string AcceptLargeObjectsValue
        {
            get
            {
                return (string)base[SystemForCrossDomainIdentityManagementConfigurationSection.PropertyNameAcceptLargeObjects];
            }
        }

        private static ISystemForCrossDomainIdentityManagementConfiguration Initialize()
        {
            ISystemForCrossDomainIdentityManagementConfiguration result =
                SystemForCrossDomainIdentityManagementConfigurationSection.Factory.Value.Create(
                    SystemForCrossDomainIdentityManagementConfigurationSection.DefaultConfiguration,
                    out ConfigurationErrorsException errors);
            return result;
        }
    }
}
