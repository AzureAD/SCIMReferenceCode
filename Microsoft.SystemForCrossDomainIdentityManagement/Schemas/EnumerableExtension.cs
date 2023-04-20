﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class EnumerableExtension
    {
        public static int CheckedCount<T>(this IEnumerable<T> enumeration)
        {
            long longCount = enumeration.LongCount();
            int result = Convert.ToInt32(longCount);
            return result;
        }
    }
}
