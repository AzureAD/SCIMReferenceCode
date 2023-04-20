// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;

    public sealed class Core2GroupJsonDeserializingFactory : JsonDeserializingFactory<Core2Group>
    {
        public override Core2Group Create(IReadOnlyDictionary<string, object> json)
        {
            if (null == json)
            {
                throw new ArgumentNullException(nameof(json));
            }

            Core2Group result = base.Create(json);

            foreach (KeyValuePair<string, object> entry in json)
            {
                if
                (
                        entry.Key.StartsWith(SchemaIdentifiers.PrefixExtension, StringComparison.OrdinalIgnoreCase)
                    && entry.Value is Dictionary<string, object> nestedObject
                )
                {
                    result.AddCustomAttribute(entry.Key, nestedObject);
                }
            }

            return result;
        }
    }
}