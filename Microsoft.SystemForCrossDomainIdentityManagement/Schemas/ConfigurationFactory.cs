// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;

    public abstract class ConfigurationFactory<TConfiguration, TException>
        where TException : Exception
    {
        public abstract TConfiguration Create(
            Lazy<TConfiguration> defaultConfiguration,
            out TException configurationError);
    }
}