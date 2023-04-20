// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    public interface IPatchOperation2Base
    {
        OperationName Name { get; set; }
        IPath Path { get; set; }
    }
}
