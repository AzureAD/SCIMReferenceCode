// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System.Collections.Generic;

    public interface IJsonSerializable
    {
        Dictionary<string, object> ToJson();
        string Serialize();

    }
}
