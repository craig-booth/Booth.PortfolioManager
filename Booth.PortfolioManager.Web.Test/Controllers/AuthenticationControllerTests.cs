using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

using Xunit;
using Moq;
using FluentAssertions;
using FluentAssertions.AspNetCore.Mvc;

using Booth.PortfolioManager.RestApi.Users;
using Booth.PortfolioManager.Web.Controllers;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Web.Authentication;
using Booth.PortfolioManager.Domain.Users;


namespace Booth.PortfolioManager.Web.Test.Controllers
{
    public class AuthenticationControllerTests
    {

        [Fact]
        public void AuthenticateFailsReturnsForbidden()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var userService = mockRepository.Create<IUserService>();
            userService.Setup(x => x.Authenticate(It.IsAny<string>(), It.IsAny<string>())).Returns(ServiceResult<User>.Error(""));
            var jwtConfig = mockRepository.Create<IJwtTokenConfigurationProvider>();

            var controller = new AuthenticationController(userService.Object, jwtConfig.Object);

            var request = new AuthenticationRequest() { UserName = "user", Password = "password" };
            var response = controller.Authenticate(request);

            response.Result.Should().BeForbidResult();

            mockRepository.Verify();
        }

        [Fact]
        public void AuthenticateSuccessfulReturnsToken()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var user = new User(Guid.NewGuid());
            user.Create("user", "password");

            var userService = mockRepository.Create<IUserService>();
            userService.Setup(x => x.Authenticate(It.IsAny<string>(), It.IsAny<string>())).Returns(ServiceResult<User>.Ok(user));
            var jwtConfig = mockRepository.Create<IJwtTokenConfigurationProvider>();
            jwtConfig.SetupGet(x => x.Issuer).Returns("TestIssuer");
            jwtConfig.SetupGet(x => x.Audience).Returns("TestAudience");
            var signingKey = new SymmetricSecurityKey(new HMACSHA256().Key);
            jwtConfig.SetupGet(x => x.Key).Returns(signingKey);

            var controller = new AuthenticationController(userService.Object, jwtConfig.Object);

            var request = new AuthenticationRequest() { UserName = "user", Password = "password" };
            var response = controller.Authenticate(request);

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters()
            {
                IssuerSigningKey = signingKey,
                ValidAudience = "TestAudience",
                ValidIssuer = "TestIssuer"
            };
            tokenHandler.ValidateToken(response.Value.Token, validationParameters, out var token);

            token.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddHours(1), 5000);


            mockRepository.Verify();
        }

        [Fact]
        public void AuthenticateTokenContainsNameClaims()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var user = new User(Guid.NewGuid());
            user.Create("user", "password");

            var userService = mockRepository.Create<IUserService>();
            userService.Setup(x => x.Authenticate(It.IsAny<string>(), It.IsAny<string>())).Returns(ServiceResult<User>.Ok(user));
            var jwtConfig = mockRepository.Create<IJwtTokenConfigurationProvider>();
            jwtConfig.SetupGet(x => x.Issuer).Returns("TestIssuer");
            jwtConfig.SetupGet(x => x.Audience).Returns("TestAudience");
            var signingKey = new SymmetricSecurityKey(new HMACSHA256().Key);
            jwtConfig.SetupGet(x => x.Key).Returns(signingKey);

            var controller = new AuthenticationController(userService.Object, jwtConfig.Object);

            var request = new AuthenticationRequest() { UserName = "user", Password = "password" };
            var response = controller.Authenticate(request);

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(response.Value.Token);

            token.Claims.Should().Contain(x => (x.Type == "nameid") &&  (x.Value == user.Id.ToString()))
                             .And.Contain(x => (x.Type == "unique_name") && (x.Value == user.UserName));

            mockRepository.Verify();
        }

        [Fact]
        public void AuthenticateTokenDoesNotAdministratorClaim()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var user = new User(Guid.NewGuid());
            user.Create("user", "password");

            var userService = mockRepository.Create<IUserService>();
            userService.Setup(x => x.Authenticate(It.IsAny<string>(), It.IsAny<string>())).Returns(ServiceResult<User>.Ok(user));
            var jwtConfig = mockRepository.Create<IJwtTokenConfigurationProvider>();
            jwtConfig.SetupGet(x => x.Issuer).Returns("TestIssuer");
            jwtConfig.SetupGet(x => x.Audience).Returns("TestAudience");
            var signingKey = new SymmetricSecurityKey(new HMACSHA256().Key);
            jwtConfig.SetupGet(x => x.Key).Returns(signingKey);

            var controller = new AuthenticationController(userService.Object, jwtConfig.Object);

            var request = new AuthenticationRequest() { UserName = "user", Password = "password" };
            var response = controller.Authenticate(request);

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(response.Value.Token);

            token.Claims.Should().NotContain(x => (x.Type == "role") && (x.Value == "Administrator"));

            mockRepository.Verify();
        }

        [Fact]
        public void AuthenticateTokenContainsAdministratorClaimForAdministrator()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var user = new User(Guid.NewGuid());
            user.Create("user", "password");
            user.AddAdministratorPrivilage();

            var userService = mockRepository.Create<IUserService>();
            userService.Setup(x => x.Authenticate(It.IsAny<string>(), It.IsAny<string>())).Returns(ServiceResult<User>.Ok(user));
            var jwtConfig = mockRepository.Create<IJwtTokenConfigurationProvider>();
            jwtConfig.SetupGet(x => x.Issuer).Returns("TestIssuer");
            jwtConfig.SetupGet(x => x.Audience).Returns("TestAudience");
            var signingKey = new SymmetricSecurityKey(new HMACSHA256().Key);
            jwtConfig.SetupGet(x => x.Key).Returns(signingKey);

            var controller = new AuthenticationController(userService.Object, jwtConfig.Object);

            var request = new AuthenticationRequest() { UserName = "user", Password = "password" };
            var response = controller.Authenticate(request);

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(response.Value.Token);

            token.Claims.Should().Contain(x => (x.Type == "role") && (x.Value == "Administrator"));

            mockRepository.Verify();
        }


    }
}
