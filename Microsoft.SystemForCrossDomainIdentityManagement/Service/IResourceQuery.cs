//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System.Collections.Generic;

    public interface IResourceQuery
    {
        IReadOnlyCollection<string> Attributes { get; }
        IReadOnlyCollection<string> ExcludedAttributes { get; }
        IReadOnlyCollection<IFilter> Filters { get; }
        IPaginationParameters PaginationParameters { get; }
    }
}