//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;

    public sealed class JsonNormalizer : JsonNormalizerTemplate
    {
        private IReadOnlyCollection<string> attributeNames;

        public override IReadOnlyCollection<string> AttributeNames
        {
            get
            {
                IReadOnlyCollection<string> result =
                    LazyInitializer.EnsureInitialized<IReadOnlyCollection<string>>(
                        ref this.attributeNames,
                        JsonNormalizer.CollectAttributeNames);
                return result;
            }
        }

        private static IReadOnlyCollection<string> CollectAttributeNames()
        {
            Type attributeNamesType = typeof(AttributeNames);
            IReadOnlyCollection<FieldInfo> members = attributeNamesType.GetFields(BindingFlags.Public | BindingFlags.Static);
            IReadOnlyCollection<string> result =
                members
                .Select(
                    (FieldInfo item) =>
                        item.GetValue(null))
                .Cast<string>()
                .ToArray();
            return result;
        }
    }
}