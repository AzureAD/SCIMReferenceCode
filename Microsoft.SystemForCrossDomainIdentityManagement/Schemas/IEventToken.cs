// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;

    public interface IEventToken
    {
        IReadOnlyCollection<string> Audience { get; set; }
        IDictionary<string, object> Events { get; }
        DateTime? Expiration { get; set; }
        JwtHeader Header { get; }
        string Identifier { get; }
        DateTime IssuedAt { get; }
        string Issuer { get; }
        DateTime? NotBefore { get; }
        string Subject { get; set; }
        string Transaction { get; set; }
    }
}