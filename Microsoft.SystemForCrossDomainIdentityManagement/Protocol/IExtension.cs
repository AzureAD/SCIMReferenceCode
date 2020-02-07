//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Net.Http;

    public interface IExtension
    {
        Type Controller { get; }
        JsonDeserializingFactory JsonDeserializingFactory { get; }
        string Path { get; }
        string SchemaIdentifier { get; }
        string TypeName { get; }

        bool Supports(HttpRequestMessage request);
    }
}