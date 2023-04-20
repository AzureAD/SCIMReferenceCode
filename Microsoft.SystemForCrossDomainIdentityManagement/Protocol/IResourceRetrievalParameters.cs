// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    public interface IResourceRetrievalParameters : IRetrievalParameters
    {
        IResourceIdentifier ResourceIdentifier { get; }
    }
}