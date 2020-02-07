//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    public interface IPaginationParameters
    {
        int? Count { get; set; }
        int? StartIndex { get; set; }
    }
}