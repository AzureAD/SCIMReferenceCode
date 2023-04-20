// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;
    using System.Globalization;

    internal static class DateTimeExtension
    {
        private const string FormatStringRoundtrip = "O";

        public static string ToRoundtripString(this DateTime dateTime)
        {
            string result = dateTime.ToString(DateTimeExtension.FormatStringRoundtrip, CultureInfo.InvariantCulture);
            return result;
        }
    }
}
