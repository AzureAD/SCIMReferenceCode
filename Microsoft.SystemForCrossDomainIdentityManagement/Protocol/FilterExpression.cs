//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    // Parses filter expressions into a doubly-linked list.
    // A collection of IFilter objects can be obtained from the fully-parsed expression.
    //
    // Brackets, that is, '(' and '),' characters demarcate groups.
    // So, each expression has a group identifier.
    // Group identifiers are integers,
    // but the group identifier may be consisted a "nominal variable,"
    // in the terminology of applied statistics: https://en.wikipedia.org/wiki/Level_of_measurement.
    // Specifically, it does not matter that group 4 is followed by group 6,
    // but merely that the expressions of group six are not in group 4.
    //
    // Brackets also demarcate levels.
    // So, each expression has a zero-based level number,
    // zero being the top level.
    // Thus, in the filter expression,
    // a eq 1 and (b eq 2 or c eq 3) and (d eq 4 or e eq 5),
    // the clause, a eq 1,
    // has the level number 0,
    // while the bracketed clauses have the level number 1.
    // The clause, a eq 1 is one group,
    // the first pair of bracketed clauses are in a second group,
    // and the second pair of bracketed clauses are in a third group.
    internal sealed class FilterExpression : IFilterExpression
    {
        private const char BracketClose = ')';
        private const char Escape = '\\';
        private const char Quote = '"';

        private const string PatternGroupLeft = "left";
        private const string PatternGroupLevelUp = "levelUp";
        private const string PatternGroupOperator = "operator";
        private const string PatternGroupRight = "right";
        // (?<levelUp>\(*)?(?<left>(\S)*)(\s*(?<operator>bitAnd|eq|ne|co|sw|ew|ge|gt|isMemberOf|lt|matchesExpression|le|notBitAnd|notMatchesExpression)\s*(?<right>(.)*))?
        private const string PatternTemplate =
            @"(?<" +
            FilterExpression.PatternGroupLevelUp +
            @">\(*)?(?<" +
            FilterExpression.PatternGroupLeft +
            @">(\S)*)(\s*(?<" +
            FilterExpression.PatternGroupOperator +
            @">{0})\s*(?<" +
            FilterExpression.PatternGroupRight +
            @">(.)*))?";

        private const string RegularExpressionOperatorOr = "|";
        private const char Space = ' ';
        private const string Template = "{0} {1} {2}";

        private static readonly Lazy<char[]> TrailingCharacters =
            new Lazy<char[]>(
                () =>
                    new char[]
                    {
                        FilterExpression.Quote,
                        FilterExpression.Space,
                        FilterExpression.BracketClose
                    });

        private static readonly Lazy<string> ComparisonOperators =
            new Lazy<string>(
                () =>
                    FilterExpression.Initialize<ComparisonOperatorValue>());

        private static readonly Lazy<string> FilterPattern =
            new Lazy<string>(
                () =>
                    FilterExpression.InitializeFilterPattern());

        private static readonly Lazy<Regex> Expression =
            new Lazy<Regex>(
                () =>
                    new Regex(FilterExpression.FilterPattern.Value, RegexOptions.CultureInvariant | RegexOptions.Compiled));

        private static readonly Lazy<string> LogicalOperatorAnd =
            new Lazy<string>(
                () =>
                    Enum.GetName(typeof(LogicalOperatorValue), LogicalOperatorValue.and));

        private static readonly Lazy<string> LogicalOperatorOr =
            new Lazy<string>(
                () =>
                    Enum.GetName(typeof(LogicalOperatorValue), LogicalOperatorValue.or));

        private string attributePath;
        private ComparisonOperatorValue comparisonOperator;
        private ComparisonOperator filterOperator;
        private int groupValue;
        private int levelValue;
        private LogicalOperatorValue logicalOperator;
        private FilterExpression next;
        private ComparisonValue value;

        private FilterExpression(FilterExpression other)
        {
            if (null == other)
            {
                throw new ArgumentNullException(nameof(other));
            }

            this.Text = other.Text;
            this.attributePath = other.attributePath;
            this.comparisonOperator = other.comparisonOperator;
            this.filterOperator = other.filterOperator;
            this.Group = other.Group;
            this.Level = other.Level;
            this.logicalOperator = other.logicalOperator;
            this.value = other.value;
            if (other.next != null)
            {
                this.next = new FilterExpression(other.next);
                this.next.Previous = this;
            }
        }

        private FilterExpression(string text, int group, int level)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentNullException(nameof(text));
            }

            this.Text = text.Trim();

            this.Level = level;
            this.Group = group;

            MatchCollection matches = FilterExpression.Expression.Value.Matches(this.Text);
            foreach (Match match in matches)
            {
                Group levelUpGroup = match.Groups[FilterExpression.PatternGroupLevelUp];
                if (levelUpGroup.Success && levelUpGroup.Value.Any())
                {
                    this.Level += levelUpGroup.Value.Length;
                    this.Group += 1;
                }
                Group operatorGroup = match.Groups[FilterExpression.PatternGroupOperator];
                if (operatorGroup.Success)
                {
                    Group leftGroup = match.Groups[FilterExpression.PatternGroupLeft];
                    Group rightGroup = match.Groups[FilterExpression.PatternGroupRight];
                    this.Initialize(leftGroup, operatorGroup, rightGroup);
                }
                else
                {
                    string remainder = match.Value.Trim();
                    if (string.IsNullOrWhiteSpace(remainder))
                    {
                        continue;
                    }
                    else if (1 == remainder.Length && FilterExpression.BracketClose == remainder[0])
                    {
                        continue;
                    }
                    else
                    {
                        throw new ArgumentException(remainder, nameof(text));
                    }
                }
            }
        }

        public FilterExpression(string text)
            : this(text: text, group: 0, level: 0)
        {
        }

        private enum ComparisonOperatorValue
        {
            bitAnd,
            eq,
            ne,
            co,
            sw,
            ew,
            ge,
            gt,
            includes,
            isMemberOf,
            lt,
            matchesExpression,
            le,
            notBitAnd,
            notMatchesExpression
        }

        private interface IComparisonValue
        {
            AttributeDataType DataType { get; }
            bool Quoted { get; }
            string Value { get; }
        }

        private enum LogicalOperatorValue
        {
            and,
            or
        }

        private int Group
        {
            get
            {
                return this.groupValue;
            }

            set
            {
                if (value < 0)
                {
                    string message =
                       string.Format(
                           CultureInfo.InvariantCulture,
                           SystemForCrossDomainIdentityManagementProtocolResources.ExceptionInvalidFilterTemplate,
                           this.Text);
#pragma warning disable CA1303 // Do not pass literals as localized parameters
                    throw new ArgumentOutOfRangeException(message, nameof(this.Group));
#pragma warning restore CA1303 // Do not pass literals as localized parameters
                }
                this.groupValue = value;
            }
        }

        private int Level
        {
            get
            {
                return this.levelValue;
            }

            set
            {
                if (value < 0)
                {
                    string message =
                       string.Format(
                           CultureInfo.InvariantCulture,
                           SystemForCrossDomainIdentityManagementProtocolResources.ExceptionInvalidFilterTemplate,
                           this.Text);
#pragma warning disable CA1303 // Do not pass literals as localized parameters
                    throw new ArgumentOutOfRangeException(message, nameof(this.Level));
#pragma warning restore CA1303 // Do not pass literals as localized parameters
                }
                this.levelValue = value;
            }
        }

        private ComparisonOperatorValue Operator
        {
            get
            {
                return this.comparisonOperator;
            }

            set
            {
                switch (value)
                {
                    case ComparisonOperatorValue.bitAnd:
                        this.filterOperator = ComparisonOperator.BitAnd;
                        break;
                    case ComparisonOperatorValue.ew:
                        this.filterOperator = ComparisonOperator.EndsWith;
                        break;
                    case ComparisonOperatorValue.eq:
                        this.filterOperator = ComparisonOperator.Equals;
                        break;
                    case ComparisonOperatorValue.ge:
                        this.filterOperator = ComparisonOperator.EqualOrGreaterThan;
                        break;
                    case ComparisonOperatorValue.gt:
                        this.filterOperator = ComparisonOperator.GreaterThan;
                        break;
                    case ComparisonOperatorValue.le:
                        this.filterOperator = ComparisonOperator.EqualOrLessThan;
                        break;
                    case ComparisonOperatorValue.lt:
                        this.filterOperator = ComparisonOperator.LessThan;
                        break;
                    case ComparisonOperatorValue.includes:
                        this.filterOperator = ComparisonOperator.Includes;
                        break;
                    case ComparisonOperatorValue.isMemberOf:
                        this.filterOperator = ComparisonOperator.IsMemberOf;
                        break;
                    case ComparisonOperatorValue.matchesExpression:
                        this.filterOperator = ComparisonOperator.MatchesExpression;
                        break;
                    case ComparisonOperatorValue.notBitAnd:
                        this.filterOperator = ComparisonOperator.NotBitAnd;
                        break;
                    case ComparisonOperatorValue.ne:
                        this.filterOperator = ComparisonOperator.NotEquals;
                        break;
                    case ComparisonOperatorValue.notMatchesExpression:
                        this.filterOperator = ComparisonOperator.NotMatchesExpression;
                        break;
                    default:
                        string notSupported = Enum.GetName(typeof(ComparisonOperatorValue), this.Operator);
                        throw new NotSupportedException(notSupported);
                }
                this.comparisonOperator = value;
            }
        }

        private FilterExpression Previous
        {
            get;
            set;
        }

        private string Text
        {
            get;
            set;
        }

        private static void And(IFilter left, IFilter right)
        {
            if (null == left)
            {
                throw new ArgumentNullException(nameof(left));
            }

            if (null == right)
            {
                throw new ArgumentNullException(nameof(right));
            }

            if (null == left.AdditionalFilter)
            {
                left.AdditionalFilter = right;
            }
            else
            {
                FilterExpression.And(left.AdditionalFilter, right);
            }
        }

        private static IReadOnlyCollection<IFilter> And(IFilter left, IReadOnlyCollection<IFilter> right)
        {
            List<IFilter> result = new List<IFilter>();
            IFilter template = new Filter(left);
            for (int index = 0; index < right.Count; index++)
            {
                IFilter rightFilter = right.ElementAt(index);
                IFilter leftFilter;
                if (0 == index)
                {
                    leftFilter = left;
                }
                else
                {
                    leftFilter = new Filter(template);
                    result.Add(leftFilter);
                }
                FilterExpression.And(leftFilter, rightFilter);
            }
            return result;
        }

        private static IReadOnlyCollection<IFilter> And(IReadOnlyCollection<IFilter> left, IFilter right)
        {
            if (null == left)
            {
                throw new ArgumentNullException(nameof(left));
            }

            if (null == right)
            {
                throw new ArgumentNullException(nameof(right));
            }

            for (int index = 0; index < left.Count; index++)
            {
                IFilter leftFilter = left.ElementAt(index);
                FilterExpression.And(leftFilter, right);
            }
            return left;
        }

        // Convert the doubly-linked list into a collection of IFilter objects.
        // There are three cases that may be encountered as the conversion proceeds through the linked list of clauses.
        // Those cases are documented by comments below.
        private IReadOnlyCollection<IFilter> Convert()
        {
            List<IFilter> result = new List<IFilter>();
            IFilter thisFilter = this.ToFilter();
            result.Add(thisFilter);
            FilterExpression current = this.next;
            while (current != null)
            {
                if (this.Level == current.Level)
                {
                    // The current clause has the same level number as the initial clause,
                    // such as
                    // b eq 2
                    // in the expression
                    // a eq 1 and b eq 2.
                    IFilter filter = current.ToFilter();
                    switch (current.Previous.logicalOperator)
                    {
                        case LogicalOperatorValue.and:
                            IFilter left = result.Last();
                            FilterExpression.And(left, filter);
                            break;
                        case LogicalOperatorValue.or:
                            result.Add(filter);
                            break;
                        default:
                            string notSupported = Enum.GetName(typeof(LogicalOperatorValue), this.logicalOperator);
                            throw new NotSupportedException(notSupported);
                    }
                    current = current.next;
                }
                else if (this.Level > current.Level)
                {
                    // The current clause has a lower level number than the initial clause,
                    // such as
                    // c eq 3
                    // in the expression
                    // (a eq 1 and b eq 2) or c eq 3.
                    IReadOnlyCollection<IFilter> superiors = current.Convert();
                    switch (current.Previous.logicalOperator)
                    {
                        case LogicalOperatorValue.and:
                            IFilter superior = superiors.First();
                            result = FilterExpression.And(result, superior).ToList();
                            IReadOnlyCollection<IFilter> remainder = superiors.Skip(1).ToArray();
                            result.AddRange(remainder);
                            break;
                        case LogicalOperatorValue.or:
                            result.AddRange(superiors);
                            break;
                        default:
                            string notSupported = Enum.GetName(typeof(LogicalOperatorValue), this.logicalOperator);
                            throw new NotSupportedException(notSupported);
                    }
                    break;
                }
                else
                {
                    // The current clause has a higher level number than the initial clause,
                    // such as
                    // b eq 2
                    // in the expression
                    // a eq 1 and (b eq 2 or c eq 3) and (d eq 4 or e eq 5)
                    //
                    // In this case, the linked list is edited,
                    // so that
                    // c eq 3
                    // has no next link,
                    // while the next link of
                    // a eq 1
                    // refers to
                    // d eq 4.
                    // Thereby,
                    // b eq 2 or c eq 3
                    // can be converted to filters and combined with the filter composed from
                    // a eq 1,
                    // after which conversion will continue with the conversion of
                    // d eq 4.
                    // It is the change in group number between
                    // c eq 3
                    // and
                    // d eq 4
                    // that identifies the end of current group,
                    // despite the two clauses having the same level number.
                    //
                    // It is because of the editing of the linked list that the public method,
                    // ToFilters(),
                    // makes a copy of the linked list before initiating conversion;
                    // so that,
                    // ToFilters()
                    // can be called on a FilterExpression any number of times,
                    // to yield the same output.
                    FilterExpression subordinate = current;
                    while (current != null && this.Level < current.Level && subordinate.Group == current.Group)
                    {
                        current = current.next;
                    }
                    if (current != null)
                    {
                        current.Previous.next = null;
                        subordinate.Previous.next = current;
                    }
                    IReadOnlyCollection<IFilter> subordinates = subordinate.Convert();
                    switch (subordinate.Previous.logicalOperator)
                    {
                        case LogicalOperatorValue.and:
                            IFilter superior = result.Last();
                            IReadOnlyCollection<IFilter> merged = FilterExpression.And(superior, subordinates);
                            result.AddRange(merged);
                            break;
                        case LogicalOperatorValue.or:
                            result.AddRange(subordinates);
                            break;
                        default:
                            string notSupported = Enum.GetName(typeof(LogicalOperatorValue), this.logicalOperator);
                            throw new NotSupportedException(notSupported);
                    }
                }
            }
            return result;
        }

        private void Initialize(Group left, Group @operator, Group right)
        {
            if (null == left)
            {
                throw new ArgumentNullException(nameof(left));
            }

            if (null == @operator)
            {
                throw new ArgumentNullException(nameof(@operator));
            }

            if (null == right)
            {
                throw new ArgumentNullException(nameof(right));
            }

            if
            (
                    !left.Success
                || !right.Success
                || string.IsNullOrEmpty(left.Value)
                || string.IsNullOrEmpty(right.Value)
            )
            {
                string message =
                    string.Format(
                        CultureInfo.InvariantCulture,
                        SystemForCrossDomainIdentityManagementProtocolResources.ExceptionInvalidFilterTemplate,
                        this.Text);
                throw new InvalidOperationException(message);
            }

            this.attributePath = left.Value;

            if (!Enum.TryParse<ComparisonOperatorValue>(@operator.Value, out ComparisonOperatorValue comparisonOperatorValue))
            {
                string message =
                    string.Format(
                        CultureInfo.InvariantCulture,
                        SystemForCrossDomainIdentityManagementProtocolResources.ExceptionInvalidFilterTemplate,
                        this.Text);
                throw new InvalidOperationException(message);
            }
            this.Operator = comparisonOperatorValue;

            if (!FilterExpression.TryParse(right.Value, out string comparisonValue))
            {
                string message =
                    string.Format(
                        CultureInfo.InvariantCulture,
                        SystemForCrossDomainIdentityManagementProtocolResources.ExceptionInvalidFilterTemplate,
                        this.Text);
                throw new InvalidOperationException(message);
            }
            this.value = new ComparisonValue(comparisonValue, FilterExpression.Quote == right.Value[0]);

            int indexRemainder = right.Value.IndexOf(comparisonValue, StringComparison.Ordinal) + comparisonValue.Length;
            if (indexRemainder >= right.Value.Length)
            {
                return;
            }
            string remainder = right.Value.Substring(indexRemainder);
            int indexAnd = remainder.IndexOf(FilterExpression.LogicalOperatorAnd.Value, StringComparison.Ordinal);
            int indexOr = remainder.IndexOf(FilterExpression.LogicalOperatorOr.Value, StringComparison.Ordinal);
            int indexNextFilter;
            int indexLogicalOperator;
            if (indexAnd >= 0 && (indexOr < 0 || indexAnd < indexOr))
            {
                indexNextFilter = indexAnd + FilterExpression.LogicalOperatorAnd.Value.Length;
                this.logicalOperator = LogicalOperatorValue.and;
                indexLogicalOperator = indexAnd;
            }
            else if (indexOr >= 0)
            {
                indexNextFilter = indexOr + FilterExpression.LogicalOperatorOr.Value.Length;
                this.logicalOperator = LogicalOperatorValue.or;
                indexLogicalOperator = indexOr;
            }
            else
            {
                string tail = remainder.Trim().TrimEnd(FilterExpression.TrailingCharacters.Value);
                if (!string.IsNullOrWhiteSpace(tail))
                {
                    string message =
                        string.Format(
                            CultureInfo.InvariantCulture,
                            SystemForCrossDomainIdentityManagementProtocolResources.ExceptionInvalidFilterTemplate,
                            this.Text);
                    throw new InvalidOperationException(message);
                }
                else
                {
                    return;
                }
            }

            string nextExpression = remainder.Substring(indexNextFilter);
            int indexClosingBracket = remainder.IndexOf(FilterExpression.BracketClose, StringComparison.InvariantCulture);
            int nextExpressionLevel;
            int nextExpressionGroup;
            if (indexClosingBracket >= 0 && indexClosingBracket < indexLogicalOperator)
            {
                nextExpressionLevel = this.Level - 1;
                nextExpressionGroup = this.Group - 1;
            }
            else
            {
                nextExpressionLevel = this.Level;
                nextExpressionGroup = this.Group;
            }
            this.next = new FilterExpression(nextExpression, nextExpressionGroup, nextExpressionLevel);
            this.next.Previous = this;
        }

        private static string Initialize<TOperator>()
        {
            Array comparisonOperatorValues = Enum.GetValues(typeof(TOperator));
            StringBuilder buffer = new StringBuilder();
            foreach (TOperator value in comparisonOperatorValues)
            {
                if (buffer.Length > 0)
                {
                    buffer.Append(FilterExpression.RegularExpressionOperatorOr);
                }
                buffer.Append(value);
            }
            string result = buffer.ToString();
            return result;
        }

        private static string InitializeFilterPattern()
        {
            string result =
                string.Format(
                    CultureInfo.InvariantCulture,
                    FilterExpression.PatternTemplate,
                    FilterExpression.ComparisonOperators.Value);
            return result;
        }

        private IFilter ToFilter()
        {
            IFilter result = new Filter(this.attributePath, this.filterOperator, this.value.Value);
            result.DataType = this.value.DataType;
            return result;
        }

        public IReadOnlyCollection<IFilter> ToFilters()
        {
            IReadOnlyCollection<IFilter> result = new FilterExpression(this).Convert();
            return result;
        }

        public override string ToString()
        {
            string result =
                string.Format(
                    CultureInfo.InvariantCulture,
                    FilterExpression.Template,
                    this.attributePath,
                    this.Operator,
                    this.value);
            return result;
        }

        // This function attempts to parse the comparison value out of the text to the right of a given comparison operator.
        // For example, given the expression,
        // a eq 1 and (b eq 2 or c eq 3) and (d eq 4 or e eq 5),
        // the text to the right of the first comparison operator will be,
        // " 1 and (b eq 2 or c eq 3) and (d eq 4 or e eq 5),"
        // and this function should yield "1" as the comparison value.
        //
        // The function aims, first, to correctly parse out arbitrarily complex comparison values that are correctly formatted.
        // Such values may include nested quotes, nested spaces and nested text matching the logical operators, "and" and "or."
        // However, for compatibility with prior behavior, the function also accepts values that are not correctly formatted,
        // but are within expressions that conform to certain assumptions.
        // For example,
        // a = Hello, World!,
        // is accepted,
        // whereas the expression should be,
        // a = "Hello, World!".
        private static bool TryParse(string input, out string comparisonValue)
        {
            comparisonValue = null;
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            string buffer;
            if (FilterExpression.Quote == input[0])
            {
                int index;
                int position = 1;
                while (true)
                {
                    index = input.IndexOf(FilterExpression.Quote, position);
                    if (index < 0)
                    {
                        throw new InvalidOperationException();
                    }
                    if (index > 1 && FilterExpression.Escape == input[index - 1])
                    {
                        position = index + 1;
                        continue;
                    }

                    // If incorrectly-escaped, string comparison values were to be rejected,
                    // which they should be, strictly,
                    // then the following check to verify that the current quote mark is the last character,
                    // or followed by a space or closing bracket,
                    // would not be necessary.
                    // Alas, invalid filters have been accepted in the past.
                    int nextCharacterIndex = index + 1;
                    if
                    (
                            nextCharacterIndex < input.Length
                        && input[nextCharacterIndex] != FilterExpression.Space
                        && input[nextCharacterIndex] != FilterExpression.BracketClose
                    )
                    {
                        position = nextCharacterIndex;
                        continue;
                    }

                    break;
                }
                buffer = input.Substring(1, index - 1);
            }
            else
            {
                int index = input.IndexOf(FilterExpression.Space, StringComparison.InvariantCulture);
                if (index >= 0)
                {
                    // If unquoted string comparison values were to be rejected,
                    // which they should be, strictly,
                    // then the following check to verify that the current space is followed by a logical operator
                    // would not be necessary.
                    // Alas, invalid filters have been accepted in the past.
                    if
                    (
                            input.LastIndexOf(FilterExpression.LogicalOperatorAnd.Value, StringComparison.Ordinal) < index
                        && input.LastIndexOf(FilterExpression.LogicalOperatorOr.Value, StringComparison.Ordinal) < index
                    )
                    {
                        buffer = input;
                    }
                    else
                    {
                        buffer = input.Substring(0, index);
                    }
                }
                else
                {
                    buffer = input;
                }
            }
            comparisonValue =
                FilterExpression.Quote == input[0] ?
                    buffer :
                    buffer.TrimEnd(FilterExpression.TrailingCharacters.Value);
            return true;
        }

        private class ComparisonValue : IComparisonValue
        {
            private const string Template = "\"{0}\"";

            public ComparisonValue(string value, bool quoted)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException(nameof(value));
                }

                this.Value = value;
                this.Quoted = quoted;

                if (this.Quoted)
                {
                    this.DataType = AttributeDataType.@string;
                }
                else if (bool.TryParse(this.Value, out bool _))
                {
                    this.DataType = AttributeDataType.boolean;
                }
                else if (long.TryParse(this.Value, out long _))
                {
                    this.DataType = AttributeDataType.integer;
                }
                else if (double.TryParse(this.Value, out double _))
                {
                    this.DataType = AttributeDataType.@decimal;
                }
                else
                {
                    this.DataType = AttributeDataType.@string;
                }
            }

            public AttributeDataType DataType
            {
                get;
            }

            public bool Quoted
            {
                get;
            }

            public string Value
            {
                get;
            }

            public override string ToString()
            {
                string result =
                    this.Quoted ?
                        string.Format(CultureInfo.InvariantCulture, ComparisonValue.Template, this.Value) :
                        this.Value;
                return result;
            }
        }
    }
}