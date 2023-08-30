// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System.Net;

    internal interface IResponse
    {
        HttpStatusCode Status { get; set; }
        string StatusCodeValue { get; set; }

        bool IsError();
    }
}