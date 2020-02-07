//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System.Collections.Generic;

    public interface IQueryParameters : IRetrievalParameters
    {
        IReadOnlyCollection<IFilter> AlternateFilters { get; }
        IPaginationParameters PaginationParameters { get; set; }
    }
}