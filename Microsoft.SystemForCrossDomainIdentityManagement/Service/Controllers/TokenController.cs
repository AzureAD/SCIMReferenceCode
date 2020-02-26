// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Text;
    using System.Web.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.IdentityModel.Tokens;

    //// Controller for generating a bearer token for authorization during testing.
    //// This is not meant to replace proper Oauth for authentication purposes.
    //[Route(ServiceConstants.RouteToken)]
    //[ApiController]
    //public class KeyController : ControllerTemplate
    //{
    //    private const int TokenLifetimeInMins = 120;

    //    public KeyController(IProvider provider, IMonitor monitor)
    //        : base(provider, monitor)
    //    {
    //    }

    //    private static string GenerateJSONWebToken()
    //    {
    //        SymmetricSecurityKey securityKey =
    //            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ServiceConstants.TokenIssuer));
    //        SigningCredentials credentials =
    //            new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    //        DateTime startTime = DateTime.UtcNow;
    //        DateTime expiryTime = startTime.AddMinutes(KeyController.TokenLifetimeInMins);

    //        JwtSecurityToken token =
    //            new JwtSecurityToken(
    //                ServiceConstants.TokenIssuer,
    //                ServiceConstants.TokenAudience,
    //                null,
    //                notBefore: startTime,
    //                expires: expiryTime,
    //                signingCredentials: credentials);

    //        string result = new JwtSecurityTokenHandler().WriteToken(token);
    //        return result;
    //    }

    //    [HttpGet]
    //    public ActionResult Get()
    //    {
    //        string tokenString = KeyController.GenerateJSONWebToken();
    //        return this.Ok(new { token = tokenString });
    //    }

    //}
}
