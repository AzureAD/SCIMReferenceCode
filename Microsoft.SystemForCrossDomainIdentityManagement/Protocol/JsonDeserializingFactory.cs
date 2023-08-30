// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System.Collections.Generic;

    public delegate Resource JsonDeserializingFactory(IReadOnlyDictionary<string, object> json);
}