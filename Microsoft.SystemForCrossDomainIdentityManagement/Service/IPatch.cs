// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM
{
    public interface IPatch
    {
        PatchRequestBase PatchRequest { get; set; }
        IResourceIdentifier ResourceIdentifier { get; set; }
    }
}
