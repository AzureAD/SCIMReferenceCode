//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

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