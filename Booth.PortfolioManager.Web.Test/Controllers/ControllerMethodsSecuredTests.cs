using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


using Xunit;
using FluentAssertions;

using Booth.Common;
using System.Linq;
using FluentAssertions.Common;
using FluentAssertions.Execution;

using Booth.PortfolioManager.Web.Authentication;

namespace Booth.PortfolioManager.Web.Test.Controllers
{
    public class ControllerMethodsSecuredTests
    {

        [Fact]
        public void AllControllerMethodsShouldRequireAuthentication()
        {
            var controllers = TypeUtils.GetSubclassesOf(typeof(ControllerBase), true)
                .Where(x => x.Assembly.GetName().Name.StartsWith("Booth.PortfolioManager.Web"))
                .Where(x => !x.IsDecoratedWith<AuthorizeAttribute>());

            using (new AssertionScope())
            {
                foreach (var controller in controllers)
                    controller.Methods().ThatAreNotDecoratedWith<AllowAnonymousAttribute>().Should().BeDecoratedWith<AuthorizeAttribute>();
            };
        }

    }
}
