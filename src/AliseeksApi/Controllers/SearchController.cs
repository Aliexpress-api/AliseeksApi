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

        public SearchController(IAliexpressService ali, IMemoryCache cache,
            IScheduler scheduler, ILogger<SearchController> logger)
        {
            this.ali = ali;
            this.cache = cache;
            this.scheduler = scheduler;
            this.logger = logger;
        }

        // GET api/search?[QueryString Args]
        //TODO: Refractor caching logic, seperate to a different class or something
        [HttpGet]
        public async Task<IEnumerable<Item>> Get([FromQuery]SearchCriteria search)
        {
            string qs = JsonConvert.SerializeObject(search);
            IEnumerable<Item> response;
            if(!cache.TryGetValue<IEnumerable<Item>>(qs, out response))
            {
                response = await ali.SearchItems(search);

                cache.Set<IEnumerable<Item>>(qs, response,
                    new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));

                logger.LogCritical("SETTING CACHE");
            }
            else
            {
                logger.LogCritical("USING CACHE");
            }

            return response;
        }

        [HttpGet]
        [Route("/api/[controller]/cache")]
        public async Task<IActionResult> Cache([FromQuery]SearchCriteria search)
        {
            string qs = JsonConvert.SerializeObject(search);
            IEnumerable<Item> dummy;
            if(!cache.TryGetValue<IEnumerable<Item>>(qs, out dummy))
            {
                var items = await ali.SearchItems(search);

                cache.Set<IEnumerable<Item>>(qs, items,
                    new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));

                logger.LogCritical("Setting cache from cache request");
            }

            return Ok();
        }
    }
}
