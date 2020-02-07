//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;

    public abstract class RetrievalParameters : IRetrievalParameters
    {
        protected RetrievalParameters(
            string schemaIdentifier,
            string path,
            IReadOnlyCollection<string> requestedAttributePaths,
            IReadOnlyCollection<string> excludedAttributePaths)
        {
            if (string.IsNullOrWhiteSpace(schemaIdentifier))
            {
                throw new ArgumentNullException(nameof(schemaIdentifier));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            this.SchemaIdentifier = schemaIdentifier;
            this.Path = path;
            this.RequestedAttributePaths =
                requestedAttributePaths ?? throw new ArgumentNullException(nameof(requestedAttributePaths));
            this.ExcludedAttributePaths =
                excludedAttributePaths ?? throw new ArgumentNullException(nameof(excludedAttributePaths));
        }

        protected RetrievalParameters(string schemaIdentifier, string path)
        {
            if (string.IsNullOrWhiteSpace(schemaIdentifier))
            {
                throw new ArgumentNullException(nameof(schemaIdentifier));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            this.SchemaIdentifier = schemaIdentifier;
            this.Path = path;
            this.RequestedAttributePaths = Array.Empty<string>();
            this.ExcludedAttributePaths = Array.Empty<string>();
        }

        public IReadOnlyCollection<string> ExcludedAttributePaths
        {
            get;
            private set;
        }

        public string Path
        {
            get;
            private set;
        }

        public IReadOnlyCollection<string> RequestedAttributePaths
        {
            get;
            private set;
        }

        public string SchemaIdentifier
        {
            get;
            private set;
        }

        public override string ToString()
        {
            string result =
                new Query
                {
                    RequestedAttributePaths = this.RequestedAttributePaths,
                    ExcludedAttributePaths = this.ExcludedAttributePaths
                }.Compose();
            return result;
        }
    }
}