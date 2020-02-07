//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    public interface IPatchOperation2Base
    {
        OperationName Name { get; set; }
        IPath Path { get; set; }
    }
}
