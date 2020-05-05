//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "Enum of type names will contain type names")]
    public enum AttributeDataType
    {
        String,
        Boolean,
        Decimal,
        Integer,
        DateTime,
        Binary,
        Reference,
        Complex
    }
}