// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public abstract class JsonNormalizerTemplate : IJsonNormalizationBehavior
    {
        public abstract IReadOnlyCollection<string> AttributeNames
        {
            get;
        }

        private IEnumerable<KeyValuePair<string, object>> Normalize(IReadOnlyCollection<KeyValuePair<string, object>> json)
        {
            if (null == json)
            {
                throw new ArgumentNullException(nameof(json));
            }

            int countElements = json.CheckedCount();
            IDictionary<string, object> result = new Dictionary<string, object>(countElements);
            foreach (KeyValuePair<string, object> element in json)
            {
                string key;
                key = element.Key;
                object value = element.Value;
                string attributeName =
                    this
                    .AttributeNames
                    .SingleOrDefault(
                        (string item) =>
                            string.Equals(item, key, StringComparison.OrdinalIgnoreCase));
                if (attributeName != null)
                {
                    if (!string.Equals(key, attributeName, StringComparison.Ordinal))
                    {
                        key = attributeName;
                    }

                    switch (value)
                    {
                        case IEnumerable<KeyValuePair<string, object>> jsonValue:
                            value = this.Normalize(jsonValue);
                            break;
                        case ArrayList jsonCollectionValue:
                            ArrayList jsonCollectionNormalized = new ArrayList();
                            foreach (object innerValue in jsonCollectionValue)
                            {
                                IEnumerable<KeyValuePair<string, object>> innerObject =
                                    innerValue as IEnumerable<KeyValuePair<string, object>>;
                                if (innerObject != null)
                                {
                                    IEnumerable<KeyValuePair<string, object>> normalizedInnerObject = this.Normalize(innerObject);
                                    jsonCollectionNormalized.Add(normalizedInnerObject);
                                }
                                else
                                {
                                    jsonCollectionNormalized.Add(innerValue);
                                }
                            }

                            value = jsonCollectionNormalized;
                            break;
                        default:
                            break;
                    }
                }

                result.Add(key, value);
            }

            return result;
        }

        private IEnumerable<KeyValuePair<string, object>> Normalize(IEnumerable<KeyValuePair<string, object>> json)
        {
            if (null == json)
            {
                throw new ArgumentNullException(nameof(json));
            }

            IReadOnlyCollection<KeyValuePair<string, object>> materializedJson = json.ToArray();
            IEnumerable<KeyValuePair<string, object>> result = this.Normalize(materializedJson);
            return result;
        }

        public IReadOnlyDictionary<string, object> Normalize(IReadOnlyDictionary<string, object> json)
        {
            if (null == json)
            {
                throw new ArgumentNullException(nameof(json));
            }

            IReadOnlyCollection<KeyValuePair<string, object>> keyedPairs =
               (IReadOnlyCollection<KeyValuePair<string, object>>)json;
            Dictionary<string, object> normalizedJson =
                this
                .Normalize(keyedPairs)
                .ToDictionary(
                    (KeyValuePair<string, object> item) =>
                        item.Key,
                    (KeyValuePair<string, object> item) =>
                        item.Value);
            IReadOnlyDictionary<string, object> result =
                new ReadOnlyDictionary<string, object>(normalizedJson);
            return result;
        }
    }
}