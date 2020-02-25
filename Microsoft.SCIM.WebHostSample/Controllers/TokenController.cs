using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.SCIM.WebHostSample.Controllers
{
    // Controller for generating a bearer token for authorization during testing.
    // This is not meant to replace proper Oauth for authentication purposes.
    [Route("scim/token")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        
        private const int defaultTokenExpiration = 120;

        public TokenController(IConfiguration Configuration)
        {
            _configuration = Configuration;
        }

        private string GenerateJSONWebToken()
        {
            // Create token key
            SymmetricSecurityKey securityKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._configuration["Token:IssuerSigningKey"]));
            SigningCredentials credentials =
                new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Set token expiration
            DateTime startTime = DateTime.UtcNow;
            DateTime expiryTime;
            double tokenExpiration;
            if (double.TryParse(this._configuration["Token:TokenLifetimeInMins"], out tokenExpiration))
                expiryTime = startTime.AddMinutes(tokenExpiration);
            else
                expiryTime = startTime.AddMinutes(defaultTokenExpiration);

            // Generate the token
            JwtSecurityToken token =
                new JwtSecurityToken(
                    this._configuration["Token:TokenIssuer"],
                    this._configuration["Token:TokenAudience"],
                    null,
                    notBefore: startTime,
                    expires: expiryTime,
                    signingCredentials: credentials);

            string result = new JwtSecurityTokenHandler().WriteToken(token);
            return result;
        }

        [HttpGet]
        public ActionResult Get()
        {
            string tokenString = this.GenerateJSONWebToken();
            return this.Ok(new { token = tokenString });
        }

    }
}
