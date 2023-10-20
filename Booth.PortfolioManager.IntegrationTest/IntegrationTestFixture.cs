using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Mongo2Go;
using Xunit;

using Booth.PortfolioManager.Web;


namespace Booth.PortfolioManager.IntegrationTest
{
    public class IntegrationTestFixture : WebApplicationFactory<Startup>
    {
        private readonly string _ConnectionString;
        private readonly string _Database = "IntegrationTest";
        private readonly MongoDbRunner _DBRunner;

        public IntegrationTestFixture()
        {
            _DBRunner = MongoDbRunner.Start();
            _ConnectionString = _DBRunner.ConnectionString;

            _DBRunner.Import(_Database, "TradingCalendar", @".\data\TradingCalendar.json", true);
            _DBRunner.Import(_Database, "Stocks", @".\data\Stocks.json", true);
            _DBRunner.Import(_Database, "StockPriceHistory", @".\data\StockPriceHistory.json", true);
            _DBRunner.Import(_Database, "Users", @".\data\Users.json", true);
            _DBRunner.Import(_Database, "Portfolios", @".\data\Portfolios.json", true);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            Environment.SetEnvironmentVariable("Settings__ConnectionString", _ConnectionString);
            Environment.SetEnvironmentVariable("Settings__Database", _Database);
            Environment.SetEnvironmentVariable("Settings__AllowDebugUserAcccess", "false");
            Environment.SetEnvironmentVariable("Settings__JwtTokenConfiguration__Issuer", "http://test.portfolio.boothfamily.id.au");
            Environment.SetEnvironmentVariable("Settings__JwtTokenConfiguration__Audience", "http://test.portfolio.boothfamily.id.au");
            Environment.SetEnvironmentVariable("Settings__JwtTokenConfiguration__Key", "/mIVOKQYG1jEnOoPLKNG8BRipGe403rSd7Pfo/xJwwScUv4bXOSERpFPkK7bmg014dKfQjxcKTOEZLeKBFn2H==");

            builder.ConfigureTestServices(x =>
            {
                x.RemoveAll<IHostedService>();
            });
        }

        private char _AsxCodeChar1 = 'A';
        private char _AsxCodeChar2 = 'A';
        private char _AsxCodeChar3 = 'A';
        public string GenerateUniqueAsxCode()
        {
            var asxCode = new String(new[] { _AsxCodeChar1, _AsxCodeChar2, _AsxCodeChar3 });

            if (_AsxCodeChar3 != 'Z')
                _AsxCodeChar3++;
            else if (_AsxCodeChar2 != 'Z')
            {
                _AsxCodeChar2++;
                _AsxCodeChar3 = 'A';
            }
            else
            {
                _AsxCodeChar1++;
                _AsxCodeChar2 = 'A';
                _AsxCodeChar3 = 'A';
            }                

            return asxCode;
        }

        protected override void Dispose(bool disposing)
        {
            _DBRunner.Dispose();
            base.Dispose(disposing);
        }
    }

    static class Integration
    {
        public const string Collection = "Integration";

        public const string User = "StandardUser";
        public const string User2 = "StandardUser2";
        public const string AdminUser = "AdminUser";
        public const string Password = "secret";

        public static Guid StockId = Guid.Parse("f12d9217-495e-42c8-8569-34b067755d04");
        public static Guid PortfolioId = Guid.Parse("4a8f36fc-494c-4747-b58f-93fa169a0f89");
    }
    
    
    [CollectionDefinition(Integration.Collection)]
    public class IntegrationCollection : ICollectionFixture<IntegrationTestFixture>
    {
    }

}
