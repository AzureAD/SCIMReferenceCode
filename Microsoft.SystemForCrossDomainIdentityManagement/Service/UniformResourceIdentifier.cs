//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    internal class UniformResourceIdentifier : IUniformResourceIdentifier
    {
        private const string AlternatePathTemplate = UniformResourceIdentifier.RegularExpressionOperatorOr + "{0}";

        private const string ArgumentNameIdentifier = "identifier";
        private const string ArgumentNameQuery = "query";

        private const string ExpressionGroupNameIdentifier = "identifier";
        private const string ExpressionGroupNameType = "type";

        private const string RegularExpressionOperatorOr = "|";

        // (?<type>(Groups|Users{0}))/?(?<identifier>[^\?]*)
        // wherein {0} will be replaced with, for example, |MyExtendedTypePath
        private const string RetrievalPatternTemplate =
            @"(?<" +
            UniformResourceIdentifier.ExpressionGroupNameType +
            @">(" +
            ProtocolConstants.PathGroups +
            UniformResourceIdentifier.RegularExpressionOperatorOr +
            ProtocolConstants.PathUsers +
            @"{0}))/?(?<" +
            UniformResourceIdentifier.ExpressionGroupNameIdentifier +
            @">[^\?]*)";

        private UniformResourceIdentifier(IResourceIdentifier identifier, IResourceQuery query)
        {
            this.Identifier = identifier ?? throw new ArgumentNullException(UniformResourceIdentifier.ArgumentNameIdentifier);
            this.Query = query ?? throw new ArgumentNullException(UniformResourceIdentifier.ArgumentNameQuery);
            this.IsQuery = null == this.Identifier || string.IsNullOrWhiteSpace(this.Identifier.Identifier);
        }

        public IResourceIdentifier Identifier
        {
            get;
            private set;
        }

        public bool IsQuery
        {
            get;
            private set;
        }

        public IResourceQuery Query
        {
            get;
            private set;
        }

        public static bool TryParse(
            Uri identifier,
            IReadOnlyCollection<IExtension> extensions,
            out IUniformResourceIdentifier parsedIdentifier)
        {
            parsedIdentifier = null;

            if (null == identifier)
            {
                throw new ArgumentNullException(UniformResourceIdentifier.ArgumentNameIdentifier);
            }

            IReadOnlyCollection<IExtension> effectiveExtensions =
                extensions ?? Array.Empty<IExtension>();

            IResourceQuery query = new ResourceQuery(identifier);

            IReadOnlyCollection<string> alternatePathCollection =
                effectiveExtensions
                .Select(
                    (IExtension item) =>
                        string.Format(CultureInfo.InvariantCulture, UniformResourceIdentifier.AlternatePathTemplate, item.Path))
                .ToArray();
            string alternatePaths = string.Join(string.Empty, alternatePathCollection);

            string retrievalPattern =
                string.Format(CultureInfo.InvariantCulture, UniformResourceIdentifier.RetrievalPatternTemplate, alternatePaths);
            Regex retrievalExpression = new Regex(retrievalPattern, RegexOptions.IgnoreCase);
            Match expressionMatch = retrievalExpression.Match(identifier.AbsoluteUri);
            if (!expressionMatch.Success)
            {
                return false;
            }

            string type = expressionMatch.Groups[UniformResourceIdentifier.ExpressionGroupNameType].Value;
            if (string.IsNullOrWhiteSpace(type))
            {
                return false;
            }

            string schemaIdentifier;
            switch (type)
            {
                case ProtocolConstants.PathGroups:
                    schemaIdentifier = SchemaIdentifiers.Core2Group;
                    break;
                case ProtocolConstants.PathUsers:
                    schemaIdentifier = SchemaIdentifiers.Core2EnterpriseUser;
                    break;
                default:
                    if (null == extensions)
                    {
                        return false;
                    }

                    schemaIdentifier =
                        effectiveExtensions
                        .Where(
                            (IExtension item) =>
                                string.Equals(item.Path, type, StringComparison.OrdinalIgnoreCase))
                        .Select(
                            (IExtension item) =>
                                item.SchemaIdentifier)
                        .SingleOrDefault();
                    if (string.IsNullOrWhiteSpace(schemaIdentifier))
                    {
                        return false;
                    }
                    break;
            }

            IResourceIdentifier resourceIdentifier = new ResourceIdentifier();
            resourceIdentifier.SchemaIdentifier = schemaIdentifier;

            string resourceIdentifierValue = expressionMatch.Groups[UniformResourceIdentifier.ExpressionGroupNameIdentifier].Value;
            if (!string.IsNullOrWhiteSpace(resourceIdentifierValue))
            {
                string unescapedIdentifier = Uri.UnescapeDataString(resourceIdentifierValue);
                resourceIdentifier.Identifier = unescapedIdentifier;
            }

            parsedIdentifier = new UniformResourceIdentifier(resourceIdentifier, query);
            return true;
        }
    }
}