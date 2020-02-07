//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;

    internal static class StringExtension
    {
        private const string PatternEscapedDoubleQuote = @"\\*" + StringExtension.QuoteDouble;
        private const string PatternEscapedSingleQuote = @"\\*" + StringExtension.QuoteSingle;
        private const string QuoteDouble = "\"";
        private const string QuoteSingle = "'";

        private static readonly Lazy<Regex> ExpressionEscapedDoubleQuote =
            new Lazy<Regex>(
                () =>
                    new Regex(StringExtension.PatternEscapedDoubleQuote, RegexOptions.Compiled | RegexOptions.CultureInvariant));
        private static readonly Lazy<Regex> ExpressionEscapedSingleQuote =
            new Lazy<Regex>(
                () =>
                    new Regex(StringExtension.PatternEscapedSingleQuote, RegexOptions.Compiled | RegexOptions.CultureInvariant));

        public static string Unquote(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            int indexQuoteDouble = input.Trim().IndexOf(StringExtension.QuoteDouble, 0, StringComparison.OrdinalIgnoreCase);
            int indexQuoteSingle = input.Trim().IndexOf(StringExtension.QuoteSingle, 0, StringComparison.OrdinalIgnoreCase);
            Regex expression;
            if (0 == indexQuoteDouble)
            {
                expression = StringExtension.ExpressionEscapedDoubleQuote.Value;
            }
            else if (0 == indexQuoteSingle)
            {
                expression = StringExtension.ExpressionEscapedSingleQuote.Value;
            }
            else
            {
                return input;
            }

            MatchCollection matches = expression.Matches(input);
            if (matches.Count <= 0)
            {
                return input;
            }

            StringBuilder buffer = new StringBuilder(input);
            for (int matchIndex = matches.Count - 1; matchIndex >= 0; matchIndex--)
            {
                Match match = matches[matchIndex];
                int index = match.Index;
                buffer.Remove(index, 1);
            }
            string result = buffer.ToString();
            return result;
        }
    }
}