using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

using Booth.PortfolioManager.RestApi.Users;
using Booth.PortfolioManager.Web.Services;


namespace Booth.PortfolioManager.Web.Controllers
{
    [ApiController]
    [Route("api")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IUserService _UserService;
        private readonly IJwtTokenConfiguration _JwtTokenConfiguration;

        public AuthenticationController(IUserService userService, IJwtTokenConfiguration jwtTokenConfiguration)
        {
            _UserService = userService;
            _JwtTokenConfiguration = jwtTokenConfiguration;
        }

        // POST : /api/authenticate       
        [AllowAnonymous]
        [Route("authenticate")]
        [HttpPost]
        public ActionResult<AuthenticationResponse> Authenticate([FromBody] AuthenticationRequest request)
        {
            var result = _UserService.Authenticate(request.UserName, request.Password);
            if (! result.Successful)
                return Forbid();

            var user = result.Result;

            // Authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _JwtTokenConfiguration.Issuer,
                Audience = _JwtTokenConfiguration.Audience,
                Expires = DateTime.UtcNow.AddHours(1),
                IssuedAt = DateTime.UtcNow,
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Role, Role.Administrator)
                }),

                SigningCredentials = new SigningCredentials(_JwtTokenConfiguration.GetKey(), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            var response = new AuthenticationResponse()
            {
                Token = tokenHandler.WriteToken(token)
            };

            return response;
        }
    }
}
