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
using Booth.PortfolioManager.Web.Authentication;
using Booth.PortfolioManager.Domain.Users;

namespace Booth.PortfolioManager.Web.Controllers
{
    [ApiController]
    [Route("api")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IUserService _UserService;
        private readonly IJwtTokenConfigurationProvider _JwtTokenConfigurationProvider;

        public AuthenticationController(IUserService userService, IJwtTokenConfigurationProvider jwtTokenConfigurationProvider)
        {
            _UserService = userService;
            _JwtTokenConfigurationProvider = jwtTokenConfigurationProvider;
        }

        // POST : /authenticate       
        [AllowAnonymous]
        [Route("authenticate")]
        [HttpPost]
        public ActionResult<AuthenticationResponse> Authenticate([FromBody] AuthenticationRequest request)
        {
            var result = _UserService.Authenticate(request.UserName, request.Password);
            if (! result.Successful)
                return Forbid();

            var user = result.Result;

            // Create claims list for the user
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            if (user.Administrator)
                claims.Add(new Claim(ClaimTypes.Role, Role.Administrator));

            // Authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _JwtTokenConfigurationProvider.Issuer,
                Audience = _JwtTokenConfigurationProvider.Audience,
                Expires = DateTime.UtcNow.AddHours(1),
                IssuedAt = DateTime.UtcNow,
                Subject = new ClaimsIdentity(claims),

                SigningCredentials = new SigningCredentials(_JwtTokenConfigurationProvider.Key, SecurityAlgorithms.HmacSha256Signature)
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
