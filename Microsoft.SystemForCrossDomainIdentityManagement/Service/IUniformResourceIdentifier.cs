// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    public interface IUniformResourceIdentifier
    {
        bool IsQuery { get; }

        IResourceIdentifier Identifier { get; }
        IResourceQuery Query { get; }
    }
}
