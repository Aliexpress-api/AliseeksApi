using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Xunit;
using AliseeksApi.UnitTests.Utility;
using Microsoft.AspNetCore.Http;

namespace AliseeksApi.UnitTests.Controllers
{
    public class SearchControllerTest
    {
        const string searchEndpoint = ApiEndpoints.Search;

        private readonly TestServer _server;
        private readonly HttpClient _client;

        public SearchControllerTest()
        {
            _server = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>());

            _client = _server.CreateClient();
        }

        [Fact]
        public async Task ReturnSearchResults()
        {
            var qs = new QueryString();
            qs.Add("searchText", "40mm+12v");

            var response = await _client.GetAsync($"{searchEndpoint}{qs.ToUriComponent()}");
        }
    }
}
