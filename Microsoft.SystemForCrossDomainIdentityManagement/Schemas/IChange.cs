// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    public interface IChange
    {
        string Watermark { get; set; }
    }
}