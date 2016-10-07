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
        public async Task<HttpResponseMessage> Get(string endpoint, Action<HttpClient> configuration = null)
        {
            var response = new HttpResponseMessage();

            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, x502, chain, policy) =>
            {
                return true;
            };

            using (HttpClient client = new HttpClient(handler))
            {
                try
                {
                    if(configuration != null)
                        configuration(client);

                    response = await client.GetAsync(endpoint);
                }
                catch(Exception e)
                {

                }
            }

            return response;
        }

        public async Task<HttpResponseMessage> Post(string endpoint, HttpContent content, Action<HttpClient> configuration = null)
        {
            var response = new HttpResponseMessage();

            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, x502, chain, policy) =>
            {
                return true;
            };

            using (HttpClient client = new HttpClient(handler))
            {
                try
                {
                    if (configuration != null)
                        configuration(client);

                    response = await client.PostAsync(endpoint, content);
                }
                catch (Exception e)
                {

                }
            }

            return response;
        }

        public async Task<HttpResponseMessage> Put(string endpoint, HttpContent content, Action<HttpClient> configuration = null)
        {
            var response = new HttpResponseMessage();

            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, x502, chain, policy) =>
            {
                return true;
            };

            using (HttpClient client = new HttpClient(handler))
            {
                try
                {
                    if (configuration != null)
                        configuration(client);

                    response = await client.PutAsync(endpoint, content);
                }
                catch (Exception e)
                {

                }
            }

            return response;
        }
    }
}
