// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System.Collections.Generic;

    public interface IRetrievalParameters
    {
        IReadOnlyCollection<string> ExcludedAttributePaths { get; }
        string Path { get; }
        IReadOnlyCollection<string> RequestedAttributePaths { get; }
        string SchemaIdentifier { get; }
    }
}