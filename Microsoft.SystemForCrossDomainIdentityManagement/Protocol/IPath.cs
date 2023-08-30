// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System.Collections.Generic;

    public interface IPath
    {
        string AttributePath { get; }
        string SchemaIdentifier { get; }
        IReadOnlyCollection<IFilter> SubAttributes { get; }
        IPath ValuePath { get; }
    }
}