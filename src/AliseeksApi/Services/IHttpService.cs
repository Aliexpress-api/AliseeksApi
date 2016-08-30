using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace AliseeksApi.Services
{
    public interface IHttpService
    {
        Task<string> Get(string endpoint);
    }
}
