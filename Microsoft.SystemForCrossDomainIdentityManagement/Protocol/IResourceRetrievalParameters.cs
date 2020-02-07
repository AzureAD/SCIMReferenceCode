//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    public interface IResourceRetrievalParameters : IRetrievalParameters
    {
        IResourceIdentifier ResourceIdentifier { get; }
    }
}