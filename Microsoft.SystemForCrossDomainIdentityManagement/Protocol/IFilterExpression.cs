//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System.Collections.Generic;

    internal interface IFilterExpression
    {
        IReadOnlyCollection<IFilter> ToFilters();
    }
}