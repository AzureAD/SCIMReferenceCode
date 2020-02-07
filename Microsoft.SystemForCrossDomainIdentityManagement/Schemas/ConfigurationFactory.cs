//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;

    public abstract class ConfigurationFactory<TConfiguration, TException>
        where TException : Exception
    {
        public abstract TConfiguration Create(
            Lazy<TConfiguration> defaultConfiguration,
            out TException error);
    }
}