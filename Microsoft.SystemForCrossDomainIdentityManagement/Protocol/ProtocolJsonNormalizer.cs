// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;

    public sealed class ProtocolJsonNormalizer : JsonNormalizerTemplate
    {
        private IReadOnlyCollection<string> attributeNames;

        public override IReadOnlyCollection<string> AttributeNames
        {
            get
            {
                IReadOnlyCollection<string> result =
                    LazyInitializer.EnsureInitialized<IReadOnlyCollection<string>>(
                        ref this.attributeNames,
                        ProtocolJsonNormalizer.CollectAttributeNames);
                return result;
            }
        }

        private static IReadOnlyCollection<string> CollectAttributeNames()
        {
            Type attributeNamesType = typeof(ProtocolAttributeNames);
            IReadOnlyCollection<FieldInfo> members = attributeNamesType.GetFields(BindingFlags.Public | BindingFlags.Static);
            IReadOnlyCollection<string> protocolAttributeNames =
                members
                .Select(
                    (FieldInfo item) =>
                        item.GetValue(null))
                .Cast<string>()
                .ToArray();
            IReadOnlyCollection<string> result =
                new JsonNormalizer()
                .AttributeNames
                .Union(protocolAttributeNames)
                .ToArray();
            return result;
        }
    }
}