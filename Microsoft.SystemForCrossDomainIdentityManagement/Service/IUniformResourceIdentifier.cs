//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    public interface IUniformResourceIdentifier
    {
        bool IsQuery { get; }

        IResourceIdentifier Identifier { get; }
        IResourceQuery Query { get; }
    }
}