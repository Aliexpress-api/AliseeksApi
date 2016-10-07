using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Net.Http;

namespace AliseeksApi.Services
{
    public interface IHttpService
    {
        Task<HttpResponseMessage> Get(string endpoint, Action<HttpClient> configuration = null);
        Task<HttpResponseMessage> Post(string endpoint, HttpContent content, Action<HttpClient> configuration = null);
        Task<HttpResponseMessage> Put(string endpoint, HttpContent content, Action<HttpClient> configuration = null);
    }
}
