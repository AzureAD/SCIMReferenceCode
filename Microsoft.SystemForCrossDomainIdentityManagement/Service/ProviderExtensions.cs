// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;

    public static class ProviderExtension
    {
        public static IReadOnlyCollection<IExtension> ReadExtensions(this IProvider provider)
        {
            if(null == provider)
            {
                throw new ArgumentNullException(nameof(provider));
            }
            IReadOnlyCollection<IExtension> result;
            try
            {
                result = provider.Extensions;
            }
            catch (NotImplementedException)
            {
                result = null;
            }
            return result;
        }
    }

}
