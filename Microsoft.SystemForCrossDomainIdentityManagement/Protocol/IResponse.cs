//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

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