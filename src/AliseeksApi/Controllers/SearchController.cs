using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AliseeksApi.Models.Search;
using AliseeksApi.Models;
using AliseeksApi.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Security.Claims;
using AliseeksApi.Services.Search;
using Microsoft.AspNetCore.Authorization;

using AliseeksApi.Storage.Postgres.Users;

namespace AliseeksApi.Controllers
{
    [Route("api/[controller]")]
    public class SearchController : Controller
    {
        private readonly ISearchService search;
        ILogger logger;

        public SearchController(ISearchService search, ILogger<SearchController> logger)
        {
            this.search = search;
            this.logger = logger;
        }

        // GET api/search?[QueryString Args]
        //TODO: Refractor caching logic, seperate to a different class or something
        [HttpGet]
        public async Task<IEnumerable<Item>> Get([FromQuery]SearchCriteria search)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
                search.Meta = new SearchCriteriaMeta()
                {
                    User = HttpContext.User.FindFirst(ClaimTypes.Name).Value
                };
            else
                search.Meta = new SearchCriteriaMeta()
                {
                    User = "Guest"
                };

            var response = await this.search.SearchItems(search);

            HttpContext.Response.Headers.Add("X-TOTAL-COUNT", response.SearchCount.ToString());

            return response.Items;
        }

        [HttpGet]
        [Route("/api/[controller]/cache")]
        public IActionResult Cache([FromQuery]SearchCriteria search)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
                search.Meta = new SearchCriteriaMeta()
                {
                    User = HttpContext.User.FindFirst(ClaimTypes.Name).Value
                };

            this.search.CacheItems(search);
            return Ok();
        }

        [HttpPost]
        [Route("/api/[controller]/save")]
        [Authorize]
        public IActionResult SaveSearch([FromBody]SearchCriteria criteria)
        {
            var model = new SavedSearchModel()
            {
                Criteria = criteria
            };

            if (HttpContext.User.Identity.IsAuthenticated)
                model.Username = HttpContext.User.FindFirst(ClaimTypes.Name).Value;

            this.search.SaveSearch(model);

            return Ok();
        }

        [HttpPost]
        [Route("/api/[controller]/price")]
        public async Task<IActionResult> GetPriceHistory([FromBody]PriceHistoryRequestModel[] models)
        {
            var response = await search.GetPriceHistories(models);

            return Json(response);
        }

        [HttpGet]
        [Route("/api/[controller]/item")]
        public async Task<IActionResult> GetSingleItem([FromQuery]SingleItemRequest request)
        {
            var response = await search.ItemSearch(request);

            return Json(response);
        }
    }
}
