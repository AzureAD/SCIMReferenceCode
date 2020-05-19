//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM.WebHostSample.Controllers
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Text;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.IdentityModel.Tokens;

    // Controller for generating a bearer token for authorization during testing.
    // This is not meant to replace proper Oauth for authentication purposes.
    [Produces("application/json")]
    [Route("scim/token")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly IWebHostEnvironment environment;
        private readonly IConfiguration configuration;
        
        private const int defaultTokenExpirationTimeInMins = 120;

        public TokenController(IWebHostEnvironment env, IConfiguration configuration)
        {
            this.configuration = configuration;
            this.environment = env;
        }

        private string GenerateJSONWebToken()
        {
            // Create token key
            SymmetricSecurityKey securityKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.configuration["Token:IssuerSigningKey"]));
            SigningCredentials credentials =
                new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Set token expiration
            DateTime startTime = DateTime.UtcNow;
            DateTime expiryTime;
            if (double.TryParse(this.configuration["Token:TokenLifetimeInMins"], out double tokenExpiration))
                expiryTime = startTime.AddMinutes(tokenExpiration);
            else
                expiryTime = startTime.AddMinutes(defaultTokenExpirationTimeInMins);

            // Generate the token
            JwtSecurityToken token =
                new JwtSecurityToken(
                    this.configuration["Token:TokenIssuer"],
                    this.configuration["Token:TokenAudience"],
                    null,
                    notBefore: startTime,
                    expires: expiryTime,
                    signingCredentials: credentials);

            string result = new JwtSecurityTokenHandler().WriteToken(token);
            return result;
        }

        /// <summary>
        /// Generate a self-signed token for endpoint testing purposes
        /// (this method is only available when running in development environment).
        /// </summary>
        /// <returns>A self-signed token</returns>
        /// <response code="200">Returns the self-signed token</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult Get()
        {
            if (this.environment.IsDevelopment())
            {
                string tokenString = this.GenerateJSONWebToken();
                return this.Ok(new { token = tokenString });
            }
            return new NotFoundResult();
        }

    }
}
