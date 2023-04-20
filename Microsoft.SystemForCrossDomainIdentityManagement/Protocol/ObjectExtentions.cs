// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public static class ObjectExtentions
    {
        public static bool IsResourceType(this object json, string scheme)
        {
            if (null == json)
            {
                throw new ArgumentNullException(nameof(json));
            }
            if (string.IsNullOrWhiteSpace(scheme))
            {
                throw new ArgumentNullException(nameof(scheme));
            }

            dynamic operationDataJson = JsonConvert.DeserializeObject(json.ToString());
            bool result = false;

            switch (operationDataJson.schemas)
            {
                case JArray schemas:
                    string[] schemasList = schemas.ToObject<string[]>();
                    result =
                        schemasList
                        .Any(
                            (string item) =>
                                string.Equals(item, scheme, StringComparison.OrdinalIgnoreCase));
                    break;
                default:
                    break;
            }
            return result;
        }
    }
}
