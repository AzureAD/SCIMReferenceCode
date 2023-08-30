// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "Enum of type names will contain type names")]
    public enum AttributeDataType
    {
        @string,
        boolean,
        @decimal,
        integer,
        dateTime,
        binary,
        reference,
        complex
    }
}
