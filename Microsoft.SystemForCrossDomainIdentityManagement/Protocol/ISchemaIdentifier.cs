// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    public interface ISchemaIdentifier
    {
        string Value { get; }

        string FindPath();
        bool TryFindPath(out string path);
    }
}