using Microsoft.AspNetCore.Mvc;
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
    [Route(ServiceConstants.RouteToken)]
    [ApiController]
    public class KeyController : ControllerBase
    {
        private const int TokenLifetimeInMins = 120;

        private static string GenerateJSONWebToken()
        {
            SymmetricSecurityKey securityKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ServiceConstants.TokenIssuer));
            SigningCredentials credentials =
                new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            DateTime startTime = DateTime.UtcNow;
            DateTime expiryTime = startTime.AddMinutes(KeyController.TokenLifetimeInMins);

            JwtSecurityToken token =
                new JwtSecurityToken(
                    ServiceConstants.TokenIssuer,
                    ServiceConstants.TokenAudience,
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
            string tokenString = KeyController.GenerateJSONWebToken();
            return this.Ok(new { token = tokenString });
        }

    }
}
