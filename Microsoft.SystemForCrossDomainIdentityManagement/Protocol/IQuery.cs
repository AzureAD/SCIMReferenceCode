//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System.Collections.Generic;

    public interface IQuery
    {
        IReadOnlyCollection<IFilter> AlternateFilters { get; set; }
        IReadOnlyCollection<string> ExcludedAttributePaths { get; set; }
        IPaginationParameters PaginationParameters { get; set; }
        string Path { get; set; }
        IReadOnlyCollection<string> RequestedAttributePaths { get; set; }

        string Compose();
    }
}