// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System.Collections.Generic;

    internal interface IFilterExpression
    {
        IReadOnlyCollection<IFilter> ToFilters();
    }
}