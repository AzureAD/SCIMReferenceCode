// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;

    public interface IUnixTime
    {
        long EpochTimestamp { get; }

        DateTime ToUniversalTime();
    }
}