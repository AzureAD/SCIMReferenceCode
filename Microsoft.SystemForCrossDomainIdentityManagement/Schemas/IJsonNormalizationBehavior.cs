// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System.Collections.Generic;

    public interface IJsonNormalizationBehavior
    {
        IReadOnlyDictionary<string, object> Normalize(IReadOnlyDictionary<string, object> json);
    }
}