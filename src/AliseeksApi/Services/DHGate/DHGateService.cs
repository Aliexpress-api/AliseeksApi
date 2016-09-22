using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Search;
using AliseeksApi.Utility;

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
            await searchItems(search);
            return null;
        }

        async Task searchItems(SearchCriteria search)
        {
            string qs = new QueryStringEncoder().CreateQueryString(search, Utility.Attributes.SearchService.DHGate);
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
        }
    }
}
