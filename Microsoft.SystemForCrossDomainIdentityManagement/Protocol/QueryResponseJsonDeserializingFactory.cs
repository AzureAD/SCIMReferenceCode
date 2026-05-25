// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class QueryResponseJsonDeserializingFactory<T> :
        ProtocolJsonDeserializingFactory<QueryResponse<T>>
        where T : Resource
    {
        public QueryResponseJsonDeserializingFactory(JsonDeserializingFactory<Schematized> jsonDeserializingFactory)
        {
            this.JsonDeserializingFactory =
                jsonDeserializingFactory ?? throw new ArgumentNullException(nameof(jsonDeserializingFactory));
        }

        private JsonDeserializingFactory<Schematized> JsonDeserializingFactory
        {
            get;
            set;
        }

        public override QueryResponse<T> Create(IReadOnlyDictionary<string, object> json)
        {
            if (null == json)
            {
                throw new ArgumentNullException(nameof(json));
            }

            if (!typeof(T).IsAbstract)
            {
                QueryResponse<T> result = base.Create(json);
                return result;
            }
            else
            {
                IReadOnlyDictionary<string, object> normalizedJson = this.Normalize(json);
                IReadOnlyDictionary<string, object> metadataJson =
                    normalizedJson
                    .Where(
                        (KeyValuePair<string, object> item) =>
                            !string.Equals(ProtocolAttributeNames.Resources, item.Key, StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(
                        (KeyValuePair<string, object> item) =>
                            item.Key,
                        (KeyValuePair<string, object> item) =>
                            item.Value);
                QueryResponse<T> result = base.Create(metadataJson);
                IReadOnlyCollection<KeyValuePair<string, object>> resourcesJson =
                    normalizedJson
                    .Where(
                        (KeyValuePair<string, object> item) =>
                            string.Equals(ProtocolAttributeNames.Resources, item.Key, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
                if (resourcesJson.Any())
                {
                    IEnumerable resourcesArray = (IEnumerable)resourcesJson.Single().Value;
                    List<T> resources = new List<T>(result.TotalResults);
                    foreach (object element in resourcesArray)
                    {
                        IReadOnlyDictionary<string, object> resourceJson = (IReadOnlyDictionary<string, object>)element;
                        T resource = (T)this.JsonDeserializingFactory.Create(resourceJson);
                        resources.Add(resource);
                    }
                    result.Resources = resources;
                }
                return result;
            }
        }
    }
}