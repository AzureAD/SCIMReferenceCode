// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    public enum ErrorType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "invalid", Justification = "The casing is stipulated in the specification of a protocol")]
        invalidFilter,

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "invalid", Justification = "The casing is stipulated in the specification of a protocol")]
        invalidPath,

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "invalid", Justification = "The casing is stipulated in the specification of a protocol")]
        invalidSyntax,

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "invalid", Justification = "The casing is stipulated in the specification of a protocol")]
        invalidValue,

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "invalid", Justification = "The casing is stipulated in the specification of a protocol")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Vers", Justification = "The name is stipulated in the specification of a protocol")]
        invalidVers,

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "mutability", Justification = "The casing is stipulated in the specification of a protocol")]
        mutability,

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "no", Justification = "The casing is stipulated in the specification of a protocol")]
        noTarget,

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "sensitive", Justification = "The casing is stipulated in the specification of a protocol")]
        sensitive,

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "too", Justification = "The casing is stipulated in the specification of a protocol")]
        tooMany,

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "uniqueness", Justification = "The casing is stipulated in the specification of a protocol")]
        uniqueness
    }
}