// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    public interface IResourceIdentifier
    {
        string Identifier { get; set; }
        string SchemaIdentifier { get; set; }
    }
}