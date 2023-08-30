// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Linq;
    using System.Web;

    public sealed class Query : IQuery
    {
        private const string AttributeNameSeparator = ",";

        public IReadOnlyCollection<IFilter> AlternateFilters
        {
            get;
            set;
        }

        public IReadOnlyCollection<string> ExcludedAttributePaths
        {
            get;
            set;
        }

        public IPaginationParameters PaginationParameters
        {
            get;
            set;
        }

        public string Path
        {
            get;
            set;
        }

        public IReadOnlyCollection<string> RequestedAttributePaths
        {
            get;
            set;
        }

        public string Compose()
        {
            string result = this.ToString();
            return result;
        }

        private static Filter Clone(IFilter filter, Dictionary<string, string> placeHolders)
        {
            string placeHolder = Guid.NewGuid().ToString();
            placeHolders.Add(placeHolder, filter.ComparisonValueEncoded);
            Filter result = new Filter(filter.AttributePath, filter.FilterOperator, placeHolder);
            if (filter.AdditionalFilter != null)
            {
                result.AdditionalFilter = Query.Clone(filter.AdditionalFilter, placeHolders);
            }
            return result;
        }

        public override string ToString()
        {
            NameValueCollection parameters = HttpUtility.ParseQueryString(string.Empty);

            if (true == this.RequestedAttributePaths?.Any())
            {
                IReadOnlyCollection<string> encodedPaths = this.RequestedAttributePaths.Encode();
                string requestedAttributes =
                    string.Join(Query.AttributeNameSeparator, encodedPaths);
                parameters.Add(QueryKeys.Attributes, requestedAttributes);
            }

            if (true == this.ExcludedAttributePaths?.Any())
            {
                IReadOnlyCollection<string> encodedPaths = this.ExcludedAttributePaths.Encode();
                string excludedAttributes =
                    string.Join(Query.AttributeNameSeparator, encodedPaths);
                parameters.Add(QueryKeys.ExcludedAttributes, excludedAttributes);
            }

            Dictionary<string, string> placeHolders;
            if (true == this.AlternateFilters?.Any())
            {
                placeHolders = new Dictionary<string, string>(this.AlternateFilters.Count);
                IReadOnlyCollection<IFilter> clones =
                    this.AlternateFilters
                    .Select(
                        (IFilter item) =>
                            Query.Clone(item, placeHolders))
                    .ToArray();
                string filters = Filter.ToString(clones);
                NameValueCollection filterParameters = HttpUtility.ParseQueryString(filters);
                foreach (string key in filterParameters.AllKeys)
                {
                    parameters.Add(key, filterParameters[key]);
                }
            }
            else
            {
                placeHolders = new Dictionary<string, string>();
            }

            if (this.PaginationParameters != null)
            {
                if (this.PaginationParameters.StartIndex.HasValue)
                {
                    string startIndex =
                        this
                        .PaginationParameters
                        .StartIndex
                        .Value
                        .ToString(CultureInfo.InvariantCulture);
                    parameters.Add(QueryKeys.StartIndex, startIndex);
                }

                if (this.PaginationParameters.Count.HasValue)
                {
                    string count =
                        this
                        .PaginationParameters
                        .Count
                        .Value
                        .ToString(CultureInfo.InvariantCulture);
                    parameters.Add(QueryKeys.Count, count);
                }
            }

            string result = parameters.ToString();
            foreach (KeyValuePair<string, string> placeholder in placeHolders)
            {
                result = result.Replace(placeholder.Key, placeholder.Value, StringComparison.InvariantCulture);
            }
            return result;
        }
    }
}