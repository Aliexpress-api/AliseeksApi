using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AliseeksApi.Models.Search;
using AliseeksApi.Models;
using AliseeksApi.Services;
using AliseeksApi.Scheduling;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AliseeksApi.Controllers
{
    [Route("api/[controller]")]
    public class SearchController : Controller
    {
        IAliexpressService ali;
        IMemoryCache cache;
        IScheduler scheduler;
        ILogger logger;

        public SearchController(IAliexpressService ali, ILogger<SearchController> logger)
        {
            this.ali = ali;
            this.logger = logger;
        }

        // GET api/search?[QueryString Args]
        //TODO: Refractor caching logic, seperate to a different class or something
        [HttpGet]
        public async Task<IEnumerable<Item>> Get([FromQuery]SearchCriteria search)
        {
            var response = await ali.SearchItems(search);

            return response;
        }

        [HttpGet]
        [Route("/api/[controller]/cache")]
        public async Task<IActionResult> Cache([FromQuery]SearchCriteria search)
        {
            await ali.CacheItems(search);
            return Ok();
        }
    }
}
