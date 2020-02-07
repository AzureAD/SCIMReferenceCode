//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class Core2EnterpriseUserJsonDeserializingFactory :
        JsonDeserializingFactory<Core2EnterpriseUser>
    {
        private static readonly Lazy<JsonDeserializingFactory<Manager>> ManagerFactory =
            new Lazy<JsonDeserializingFactory<Manager>>(
                () =>
                    new ManagerDeserializingFactory());

        public override Core2EnterpriseUser Create(IReadOnlyDictionary<string, object> json)
        {
            if (null == json)
            {
                throw new ArgumentNullException(nameof(json));
            }

            Manager manager;
            IReadOnlyDictionary<string, object> normalizedJson = this.Normalize(json);
            IReadOnlyDictionary<string, object> safeJson;
            if (normalizedJson.TryGetValue(AttributeNames.Manager, out object managerData)
                && managerData != null)
            {
                safeJson =
                    normalizedJson
                    .Where(
                        (KeyValuePair<string, object> item) =>
                            !string.Equals(AttributeNames.Manager, item.Key, StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(
                        (KeyValuePair<string, object> item) =>
                            item.Key,
                        (KeyValuePair<string, object> item) =>
                            item.Value);
                switch (managerData)
                {
                    case string value:
                        manager =
                            new Manager()
                            {
                                Value = value
                            };
                        break;
                    case Dictionary<string, object> managerJson:
                        manager = Core2EnterpriseUserJsonDeserializingFactory.ManagerFactory.Value.Create(managerJson);
                        break;
                    default:
                        throw new NotSupportedException(managerData.GetType().FullName);
                }
            }
            else
            {
                safeJson =
                    normalizedJson
                    .ToDictionary(
                        (KeyValuePair<string, object> item) =>
                            item.Key,
                        (KeyValuePair<string, object> item) =>
                            item.Value);
                manager = null;
            }

            Core2EnterpriseUser result = base.Create(safeJson);

            foreach (KeyValuePair<string, object> entry in json)
            {
                if
                (
                        entry.Key.StartsWith(SchemaIdentifiers.PrefixExtension, StringComparison.OrdinalIgnoreCase)
                    && !entry.Key.StartsWith(SchemaIdentifiers.Core2EnterpriseUser, StringComparison.OrdinalIgnoreCase)
                    && entry.Value is Dictionary<string, object> nestedObject
                )
                {
                    result.AddCustomAttribute(entry.Key, nestedObject);
                }
            }

            return result;
        }

        private class ManagerDeserializingFactory : JsonDeserializingFactory<Manager>
        {
        }
    }
}