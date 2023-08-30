// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    public static class IReadOnlyCollectionExtensions
    {
        public static IReadOnlyCollection<string> Encode(this IReadOnlyCollection<string> collection)
        {
            IReadOnlyCollection<string> result =
                collection
                .Select(
                    (string item) =>
                        HttpUtility.UrlEncode(item))
                .ToArray();
            return result;
        }

        public static bool TryGetPath(
            this IReadOnlyCollection<IExtension> extensions,
            string schemaIdentifier,
            out string path)
        {
            if (string.IsNullOrWhiteSpace(schemaIdentifier))
            {
                throw new ArgumentNullException(nameof(schemaIdentifier));
            }

            path = null;
            IExtension
                extension =
                extensions
                .SingleOrDefault(
                    (IExtension item) =>
                        string.Equals(schemaIdentifier, item.SchemaIdentifier, StringComparison.OrdinalIgnoreCase));
            if (null == extension)
            {
                return false;
            }

            path = extension.Path;
            return true;
        }
    }
}