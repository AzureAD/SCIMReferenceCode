//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Configuration;

    public class ConfigurationSectionFactory<TConfiguration> :
        ConfigurationFactory<TConfiguration, ConfigurationErrorsException>
        where TConfiguration : ConfigurationSection
    {
        public ConfigurationSectionFactory(string sectionName)
        {
            if (string.IsNullOrEmpty(sectionName))
            {
                throw new ArgumentNullException(nameof(sectionName));
            }

            this.SectionName = sectionName;
        }

        private string SectionName
        {
            get;
            set;
        }

        public override TConfiguration Create(
            Lazy<TConfiguration> defaultConfiguration,
            out ConfigurationErrorsException errors)
        {
            errors = null;

            if (null == defaultConfiguration)
            {
                throw new ArgumentNullException(nameof(defaultConfiguration));
            }

            TConfiguration result = null;
            try
            {
                result = (TConfiguration)ConfigurationManager.GetSection(this.SectionName);
            }
            catch (ConfigurationErrorsException exception)
            {
                errors = exception;
            }

            if (null == result)
            {
                result = defaultConfiguration.Value;
            }

            return result;
        }
    }
}
