// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    public interface IPaginationParameters
    {
        int? Count { get; set; }
        int? StartIndex { get; set; }
    }
}