// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System.Collections.Generic;

    public interface IQueryParameters : IRetrievalParameters
    {
        IReadOnlyCollection<IFilter> AlternateFilters { get; }
        IPaginationParameters PaginationParameters { get; set; }
    }
}