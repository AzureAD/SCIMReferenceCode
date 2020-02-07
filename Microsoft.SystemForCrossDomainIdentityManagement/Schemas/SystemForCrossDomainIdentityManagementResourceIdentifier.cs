//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    public sealed class SystemForCrossDomainIdentityManagementResourceIdentifier :
        ISystemForCrossDomainIdentityManagementResourceIdentifier
    {
        private const string SeparatorSegments = "/";

        private static readonly Lazy<string[]> SeperatorsSegments =
            new Lazy<string[]>(
                () =>
                    new string[]
                        {
                            SystemForCrossDomainIdentityManagementResourceIdentifier.SeparatorSegments
                        });

        public SystemForCrossDomainIdentityManagementResourceIdentifier(Uri identifier)
        {
            if (null == identifier)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            string path = identifier.OriginalString;

            // System.Uri.Segments is not supported for relative identifiers.  
            var segmentsIndexed =
                path.Split(SystemForCrossDomainIdentityManagementResourceIdentifier.SeperatorsSegments.Value, StringSplitOptions.None)
                .Select(
                    (string item, int index) =>
                        new
                        {
                            Segment = item,
                            Index = index
                        })
                .ToArray(); ;

            var segmentSystemForCrossDomainIdentityManagement =
                segmentsIndexed
                .LastOrDefault(
                    (item) =>
                        item.Segment.Equals(SchemaConstants.PathInterface, StringComparison.OrdinalIgnoreCase));
            if (null == segmentSystemForCrossDomainIdentityManagement)
            {
                if (identifier.IsAbsoluteUri)
                {
                    string exceptionMessage =
                        string.Format(
                            CultureInfo.InvariantCulture,
                            SystemForCrossDomainIdentityManagementSchemasResources.ExceptionInvalidIdentifierTemplate,
                            path);
                    throw new ArgumentException(exceptionMessage);
                }
            }
            else
            {
                segmentsIndexed =
                    segmentsIndexed
                    .Where(
                        (item) =>
                            item.Index > segmentSystemForCrossDomainIdentityManagement.Index)
                    .ToArray();
            }

            IReadOnlyCollection<string> segmentsRelative =
                segmentsIndexed
                .Select(
                    (item) =>
                        item.Segment)
                .ToArray();

            string relativePath = string.Join(SystemForCrossDomainIdentityManagementResourceIdentifier.SeparatorSegments, segmentsRelative);

            if (!relativePath.StartsWith(SystemForCrossDomainIdentityManagementResourceIdentifier.SeparatorSegments, StringComparison.OrdinalIgnoreCase))
            {
                relativePath = string.Concat(SystemForCrossDomainIdentityManagementResourceIdentifier.SeparatorSegments, relativePath);
            }

            this.RelativePath = relativePath;
        }

        public string RelativePath
        {
            get;
            private set;
        }
    }
}
