using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Search;
using AliseeksApi.Utility;
using Microsoft.AspNetCore.Http;

namespace AliseeksApi.Services.DHGate
{
    public class DHGateService : IDHGateService
    {
        private readonly IHttpService http;

        public DHGateService(IHttpService http)
        {
            this.http = http;
        }

        public async Task<SearchResultOverview> SearchItems(SearchCriteria search)
        {
            return await searchItems(search);
        }

        async Task<SearchResultOverview> searchItems(SearchCriteria search)
        {
            string qs = new QueryStringEncoder().CreateQueryString(search, Utility.Attributes.SearchService.DHGate);

            var uniqueQs = new QueryString();
            uniqueQs.Add("")

            string endpoint = SearchEndpoints.DHGateSearchUrl + qs;
            var items = new SearchResultOverview();

            try
            {
                var response = await http.Get(endpoint);

                items = new DHGatePageDecoder().DecodePage(response);
            }
            catch(Exception e)
            {

            }

            return items;
        }
    }
}
