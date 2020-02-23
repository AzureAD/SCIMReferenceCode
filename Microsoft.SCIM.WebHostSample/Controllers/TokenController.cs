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
        //private const int TokenLifetimeInMins = 120;

        public TokenController(IConfiguration Configuration)
        {
            _configuration = Configuration;
        }

        private string GenerateJSONWebToken()
        {
            SymmetricSecurityKey securityKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._configuration["Token:IssuerSigningKey"]));
            SigningCredentials credentials =
                new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            DateTime startTime = DateTime.UtcNow;
            DateTime expiryTime = startTime.AddMinutes(double.Parse(this._configuration["Token:TokenLifetimeInMins"]));

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
