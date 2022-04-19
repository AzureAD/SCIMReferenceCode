//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web;

    public sealed class Filter : IFilter
    {
        private const string ComparisonValueTemplate = "\"{0}\"";

        private const string EncodingSpacePer2396 = "+";

        public const string NullValue = "null";

        private const string ReservedPerRfc2396 = ";/?:@&=+$,";
        private const string ReservedPerRfc3986 = Filter.ReservedPerRfc2396 + "#[]!'()*";

        private const string Space = " ";

        private const string Template = "filter={0}";
        private const string TemplateComparison = "{0} {1} {2}";
        private const string TemplateConjunction = "{0} {1} {2}";
        private const string TemplateNesting = "({0})";

        private static readonly Lazy<char[]> ReservedCharactersPerRfc3986 =
            new Lazy<char[]>(
                () =>
                    Filter.ReservedPerRfc3986.ToCharArray());
        private static readonly Lazy<IReadOnlyDictionary<string, string>> ReservedCharacterEncodingsPerRfc3986 =
            new Lazy<IReadOnlyDictionary<string, string>>(
                () =>
                    Filter.InitializeReservedCharacter3986Encodings());
        private static readonly Lazy<IReadOnlyDictionary<string, string>> ReservedCharacterEncodingsPerRfc2396 =
            new Lazy<IReadOnlyDictionary<string, string>>(
                () =>
                    Filter.InitializeReservedCharacter2396Encodings());

        private string comparisonValue;
        private string comparisonValueEncoded;
        private AttributeDataType? dataType;

        private Filter()
        {
        }

        public Filter(string attributePath, ComparisonOperator filterOperator, string comparisonValue)
        {
            if (string.IsNullOrWhiteSpace(attributePath))
            {
                throw new ArgumentNullException(nameof(attributePath));
            }

            if (string.IsNullOrWhiteSpace(comparisonValue))
            {
                throw new ArgumentNullException(nameof(comparisonValue));
            }

            this.AttributePath = attributePath;
            this.FilterOperator = filterOperator;
            this.ComparisonValue = comparisonValue;
            this.DataType = AttributeDataType.@string;
        }

        public Filter(IFilter other)
            : this(other?.AttributePath, other.FilterOperator, other?.ComparisonValue)
        {
            if (null == other)
            {
                throw new ArgumentNullException(nameof(other));
            }

            this.DataType = other.DataType;

            if (other.AdditionalFilter != null)
            {
                this.AdditionalFilter = new Filter(other.AdditionalFilter);
            }
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

        private enum LogicalOperatorValue
        {
            and,
            or
        }

        public IFilter AdditionalFilter
        {
            get;
            set;
        }

        public string AttributePath
        {
            get;
            private set;
        }

        public string ComparisonValue
        {
            get
            {
                return this.comparisonValue;
            }

            private set
            {
                Filter.Validate(this.DataType, value);
                this.comparisonValue = value;
                string encodedValue = this.comparisonValue;
                foreach (KeyValuePair<string, string> encoding in Filter.ReservedCharacterEncodingsPerRfc2396.Value)
                {
                    encodedValue = encodedValue.Replace(encoding.Key, encoding.Value, StringComparison.InvariantCulture);
                }
                this.comparisonValueEncoded = encodedValue;
            }
        }

        public string ComparisonValueEncoded
        {
            get
            {
                return this.comparisonValueEncoded;
            }
        }

        public AttributeDataType? DataType
        {
            get
            {
                return this.dataType;
            }

            set
            {
                Filter.Validate(value, this.ComparisonValue);
                this.dataType = value;
            }
        }

        public ComparisonOperator FilterOperator
        {
            get;
            set;
        }

        private static IReadOnlyDictionary<string, string> InitializeReservedCharacter2396Encodings()
        {
            Dictionary<string, string> result =
                Filter.ReservedCharacterEncodingsPerRfc3986.Value
                .ToDictionary(
                    (KeyValuePair<string, string> item) => item.Key,
                    (KeyValuePair<string, string> item) => item.Value);
            result.Add(Filter.Space, Filter.EncodingSpacePer2396);
            return result;
        }

        private static IReadOnlyDictionary<string, string> InitializeReservedCharacter3986Encodings()
        {
            Dictionary<string, string> result =
                new Dictionary<string, string>(Filter.ReservedCharactersPerRfc3986.Value.Length);
            foreach (char character in Filter.ReservedCharactersPerRfc3986.Value)
            {
                string from = character.ToString(CultureInfo.InvariantCulture);
                string to = HttpUtility.UrlEncode(from);
                result.Add(from, to);
            }
            return result;
        }

        public string Serialize()
        {
            ComparisonOperatorValue operatorValue;
            switch (this.FilterOperator)
            {
                case ComparisonOperator.BitAnd:
                    operatorValue = ComparisonOperatorValue.bitAnd;
                    break;
                case ComparisonOperator.EndsWith:
                    operatorValue = ComparisonOperatorValue.ew;
                    break;
                case ComparisonOperator.Equals:
                    operatorValue = ComparisonOperatorValue.eq;
                    break;
                case ComparisonOperator.EqualOrGreaterThan:
                    operatorValue = ComparisonOperatorValue.ge;
                    break;
                case ComparisonOperator.GreaterThan:
                    operatorValue = ComparisonOperatorValue.gt;
                    break;
                case ComparisonOperator.EqualOrLessThan:
                    operatorValue = ComparisonOperatorValue.le;
                    break;
                case ComparisonOperator.LessThan:
                    operatorValue = ComparisonOperatorValue.lt;
                    break;
                case ComparisonOperator.Includes:
                    operatorValue = ComparisonOperatorValue.includes;
                    break;
                case ComparisonOperator.IsMemberOf:
                    operatorValue = ComparisonOperatorValue.isMemberOf;
                    break;
                case ComparisonOperator.MatchesExpression:
                    operatorValue = ComparisonOperatorValue.matchesExpression;
                    break;
                case ComparisonOperator.NotBitAnd:
                    operatorValue = ComparisonOperatorValue.notBitAnd;
                    break;
                case ComparisonOperator.NotEquals:
                    operatorValue = ComparisonOperatorValue.ne;
                    break;
                case ComparisonOperator.NotMatchesExpression:
                    operatorValue = ComparisonOperatorValue.notMatchesExpression;
                    break;
                default:
                    string notSupportedValue = Enum.GetName(typeof(ComparisonOperator), this.FilterOperator);
                    throw new NotSupportedException(notSupportedValue);
            }

            string rightHandSide;
            AttributeDataType effectiveDataType = this.DataType ?? AttributeDataType.@string;
            switch (effectiveDataType)
            {
                case AttributeDataType.boolean:
                case AttributeDataType.@decimal:
                case AttributeDataType.integer:
                    rightHandSide = this.ComparisonValue;
                    break;
                default:
                    rightHandSide =
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Filter.ComparisonValueTemplate,
                            this.ComparisonValue);
                    break;
            }

            string filter =
                string.Format(
                    CultureInfo.InvariantCulture,
                    Filter.TemplateComparison,
                    this.AttributePath,
                    operatorValue,
                    rightHandSide);
            string result;
            if (this.AdditionalFilter != null)
            {
                string additionalFilter = this.AdditionalFilter.Serialize();
                result =
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Filter.TemplateConjunction,
                        filter,
                        Filter.LogicalOperatorValue.and,
                        additionalFilter);
            }
            else
            {
                result = filter;
            }
            return result;
        }

        public override string ToString()
        {
            string result = this.Serialize();
            return result;
        }

        public static string ToString(IReadOnlyCollection<IFilter> filters)
        {
            if (null == filters)
            {
                throw new ArgumentNullException(nameof(filters));
            }

            string placeholder = Guid.NewGuid().ToString();
            string allFilters = null;
            foreach (IFilter filter in filters)
            {
                Filter clone = new Filter(filter);
                clone.ComparisonValue = placeholder;
                string currentFilter = clone.Serialize();
                string encodedFilter = 
                    HttpUtility
                    .UrlEncode(currentFilter)
                    .Replace(placeholder, filter.ComparisonValueEncoded, StringComparison.InvariantCulture);
                if (string.IsNullOrWhiteSpace(allFilters))
                {
                    allFilters =
                        filters.Count > 1 ?
                            string.Format(CultureInfo.InvariantCulture, Filter.TemplateNesting, encodedFilter) :
                            encodedFilter;
                }
                else
                {
                    string rightHandSide =
                        filter.AdditionalFilter != null || filters.Count > 1 ?
                            string.Format(CultureInfo.InvariantCulture, Filter.TemplateNesting, encodedFilter) :
                            encodedFilter;
                    allFilters =
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Filter.TemplateConjunction,
                            allFilters,
                            Filter.LogicalOperatorValue.or,
                            rightHandSide);
                }
            }

            string result =
                string.Format(
                    CultureInfo.InvariantCulture,
                    Filter.Template,
                    allFilters);
            return result;
        }

        public static bool TryParse(string filterExpression, out IReadOnlyCollection<IFilter> filters)
        {
            string expression = filterExpression?.Trim()?.Unquote();

            if (string.IsNullOrWhiteSpace(expression))
            {
                throw new ArgumentNullException(nameof(filterExpression));
            }

            try
            {
                IReadOnlyCollection<IFilter> buffer = new FilterExpression(expression).ToFilters();
                filters = buffer;
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                filters = null;
                return false;
            }
            catch (ArgumentException)
            {
                filters = null;
                return false;
            }
            catch (InvalidOperationException)
            {
                filters = null;
                return false;
            }
        }

        private static void Validate(AttributeDataType? dataType, string value)
        {
            if (!dataType.HasValue || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            switch (dataType.Value)
            {
                case AttributeDataType.boolean:
                    if (!bool.TryParse(value, out bool _))
                    {
                        throw new InvalidOperationException(
                            SystemForCrossDomainIdentityManagementProtocolResources.ExceptionInvalidValue);
                    }
                    break;
                case AttributeDataType.@decimal:
                    if (!double.TryParse(value, out double _))
                    {
                        throw new InvalidOperationException(
                            SystemForCrossDomainIdentityManagementProtocolResources.ExceptionInvalidValue);
                    }
                    break;
                case AttributeDataType.integer:
                    if (!long.TryParse(value, out long _))
                    {
                        throw new InvalidOperationException(
                            SystemForCrossDomainIdentityManagementProtocolResources.ExceptionInvalidValue);
                    }
                    break;
                case AttributeDataType.binary:
                case AttributeDataType.complex:
                case AttributeDataType.dateTime:
                case AttributeDataType.reference:
                case AttributeDataType.@string:
                    break;
                default:
                    string unsupported = Enum.GetName(typeof(AttributeDataType), dataType.Value);
                    throw new NotSupportedException(unsupported);
            }
        }
    }
}