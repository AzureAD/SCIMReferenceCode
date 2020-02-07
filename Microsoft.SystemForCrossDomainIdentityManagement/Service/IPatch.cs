//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    public interface IPatch
    {
        PatchRequestBase PatchRequest { get; set; }
        IResourceIdentifier ResourceIdentifier { get; set; }
    }
}