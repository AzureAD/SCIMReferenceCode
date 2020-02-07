//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;

    public sealed class QueryParameters : RetrievalParameters, IQueryParameters
    {
        public QueryParameters(
            string schemaIdentifier,
            string path,
            IFilter filter,
            IReadOnlyCollection<string> requestedAttributePaths,
            IReadOnlyCollection<string> excludedAttributePaths)
            : base(schemaIdentifier, path, requestedAttributePaths, excludedAttributePaths)
        {
            if (null == filter)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            this.AlternateFilters = filter.ToCollection();
        }

        public QueryParameters(
            string schemaIdentifier,
            string path,
            IReadOnlyCollection<IFilter> alternateFilters,
            IReadOnlyCollection<string> requestedAttributePaths,
            IReadOnlyCollection<string> excludedAttributePaths)
            : base(schemaIdentifier, path, requestedAttributePaths, excludedAttributePaths)
        {
            this.AlternateFilters = alternateFilters ?? throw new ArgumentNullException(nameof(alternateFilters));
        }

        public QueryParameters(
            string schemaIdentifier,
            string path,
            IPaginationParameters paginationParameters)
            : this(schemaIdentifier, path, Array.Empty<IFilter>(), Array.Empty<string>(), Array.Empty<string>())
        {
            this.PaginationParameters = paginationParameters ?? throw new ArgumentNullException(nameof(paginationParameters));
        }

        [Obsolete("Use QueryParameters(string, string, IFilter, IReadOnlyCollection<string>, IReadOnlyCollection<string>) instead")]
        public QueryParameters(
            string schemaIdentifier,
            IFilter filter,
            IReadOnlyCollection<string> requestedAttributePaths,
            IReadOnlyCollection<string> excludedAttributePaths)
            : this(
                schemaIdentifier,
                new SchemaIdentifier(schemaIdentifier).FindPath(),
                filter,
                requestedAttributePaths,
                excludedAttributePaths)
        {
        }

        [Obsolete("Use QueryParameters(string, string, IReadOnlyCollection<IFilter>, IReadOnlyCollection<string>, IReadOnlyCollection<string>) instead")]
        public QueryParameters(
            string schemaIdentifier,
            IReadOnlyCollection<IFilter> alternateFilters,
            IReadOnlyCollection<string> requestedAttributePaths,
            IReadOnlyCollection<string> excludedAttributePaths)
            : this(
                schemaIdentifier,
                new SchemaIdentifier(schemaIdentifier).FindPath(),
                alternateFilters,
                requestedAttributePaths,
                excludedAttributePaths)
        {
        }

        public IReadOnlyCollection<IFilter> AlternateFilters
        {
            get;
            private set;
        }

        public IPaginationParameters PaginationParameters
        {
            get;
            set;
        }

        public override string ToString()
        {
            string result =
                new Query
                {
                    AlternateFilters = this.AlternateFilters,
                    RequestedAttributePaths = this.RequestedAttributePaths,
                    ExcludedAttributePaths = this.ExcludedAttributePaths,
                    PaginationParameters = this.PaginationParameters
                }.Compose();
            return result;
        }
    }
}