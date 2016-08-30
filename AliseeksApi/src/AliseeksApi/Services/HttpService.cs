using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Net.Http;

namespace AliseeksApi.Services
{
    public class HttpService : IHttpService
    {
        public async Task<string> Get(string endpoint)
        {
            HttpContent content = null;

            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(endpoint);
                content = response.Content;
            }

            return await content.ReadAsStringAsync();
        }
    }
}
