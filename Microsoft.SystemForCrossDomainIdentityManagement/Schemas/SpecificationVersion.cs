// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;

    public static class SpecificationVersion
    {
        private static readonly Lazy<Version> VersionOneOhValue =
            new Lazy<System.Version>(
                () =>
                    new Version(1, 0));

        private static readonly Lazy<Version> VersionOneOneValue =
            new Lazy<Version>(
                () =>
                    new Version(1, 1));

        private static readonly Lazy<Version> VersionTwoOhValue =
            new Lazy<Version>(
                () =>
                    new Version(2, 0));

        // NOTE: This version is to be used for DCaaS only.
        private static readonly Lazy<Version> VersionTwoOhOneValue =
            new Lazy<Version>(
                () =>
                    new Version(2, 0, 1));

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Oh", Justification = "Not an abbreviation")]
        public static Version VersionOneOh
        {
            get
            {
                return SpecificationVersion.VersionOneOhValue.Value;
            }
        }

        public static Version VersionOneOne
        {
            get
            {
                return SpecificationVersion.VersionOneOneValue.Value;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Oh", Justification = "Not an abbreviation")]
        public static Version VersionTwoOhOne
        {
            get
            {
                return SpecificationVersion.VersionTwoOhOneValue.Value;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Oh", Justification = "Not an abbreviation")]
        public static Version VersionTwoOh
        {
            get
            {
                return SpecificationVersion.VersionTwoOhValue.Value;
            }
        }
    }
}