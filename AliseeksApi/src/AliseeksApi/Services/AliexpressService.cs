using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models;
using AliseeksApi.Models.Search;
using AliseeksApi.Utility;

namespace AliseeksApi.Services
{
    public class AliexpressService : IAliexpressService
    {
        IHttpService http;

        public AliexpressService(IHttpService http)
        {
            this.http = http;
        }

        public async Task<IEnumerable<Item>> SearchItems(SearchCriteria search)
        {
            string qs = new AliSearchEncoder().CreateQueryString(search);
            string endpoint = AliexpressEndpoints.SearchUrl + qs;

            var response = await http.Get(endpoint);

            var items = new AliexpressPageDecoder().DecodePage(response);

            return items;
        }
    }
}
