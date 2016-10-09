﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SharpRaven.Core;
using AliseeksApi.Services.Dropshipping;
using AliseeksApi.Models.Search;
using Microsoft.AspNetCore.Authorization;
using AliseeksApi.Models.Dropshipping;
using AliseeksApi.Services.Aliexpress;
using AliseeksApi.Utility.Extensions;
using AliseeksApi.Services.User;
using AliseeksApi.Models.User;
using AliseeksApi.Services.Dropshipping.Shopify;
using AliseeksApi.Models.Shopify;
using AliseeksApi.Storage.Postgres.OAuth;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace AliseeksApi.Controllers
{
    //[Authorize]
    public class DropshippingController : Controller
    {
        private readonly DropshippingService dropship;
        private readonly IRavenClient raven;
        private readonly IUserService user;
        private readonly ShopifyService shopify;

        public DropshippingController(DropshippingService dropship, ShopifyService shopify, IUserService user, IRavenClient raven)
        {
            this.dropship = dropship;
            this.raven = raven;
            this.user = user;
            this.shopify = shopify;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("/api/[controller]/add")]
        public async Task<IActionResult> Add([FromBody]SingleItemRequest item)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
                item.Username = HttpContext.User.Identity.Name;
            else
                item.Username = "Guest";

            await dropship.AddProduct(item.Username, item);

            return Ok();
        }

        [HttpPut]
        [Route("/api/[controller]/update")]
        public async Task<IActionResult> Update([FromBody]DropshipItemModel model)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
                model.Username = HttpContext.User.Identity.Name;
            else
                model.Username = "Guest";

            await dropship.Update(model);

            return Ok();
        }

        [HttpGet]
        [Route("/api/[controller]")]
        public async Task<IActionResult> Get()
        {
            var username = "Guest";

            if (HttpContext.User.Identity.IsAuthenticated)
                username = HttpContext.User.Identity.Name;

            var items = await dropship.GetProducts(username);

            return Json(items);
        }

        [HttpGet]
        [Route("/api/[controller]/overview")]
        public async Task<IActionResult> GetOverview()
        {
            var username = "Guest";

            if (HttpContext.User.Identity.IsAuthenticated)
                username = HttpContext.User.Identity.Name;

            var overview = await dropship.GetOverview(username);

            return Json(overview);
        }

        [HttpGet]
        [Route("/api/[controller]/account")]
        public async Task<IActionResult> GetAccount()
        {
            var username = "Guest";

            if (HttpContext.User.Identity.IsAuthenticated)
                username = HttpContext.User.Identity.Name;

            var account = await dropship.GetAccount(username);

            return Json(account);
        }

        [HttpGet]
        [Route("/api/[controller]/account/orders")]
        public async Task<IActionResult> GetOrders()
        {
            var username = "Guest";

            if (HttpContext.User.Identity.IsAuthenticated)
                username = HttpContext.User.Identity.Name;

            var orders = await dropship.GetOrders(username);

            return Json(orders);
        }

        [HttpGet]
        [Route("/api/[controller]/account/shopify/oauth")]
        public IActionResult GetShopifyOAuth(string shop, string redirect, [FromServices]ShopifyOAuth oauth)
        {
            var request = oauth.GetOAuthRequest(shop, redirect);

            return Json(request);
        }

        [HttpPost]
        [Route("/api/[controller]/account/shopify/oauth")]
        public async Task<IActionResult> CreateShopifyOAuth([FromBody]ShopifyOAuthResponse response, [FromServices]ShopifyOAuth oauth)
        {
            if (!oauth.VerifyOAuthRequest(response))
                return NotFound();

            var username = String.Empty;

            if (HttpContext.User.Identity.IsAuthenticated)
                username = HttpContext.User.Identity.Name;

            if (!await shopify.AddShopifyIntegration(username, response, oauth))
                return NotFound();

            return Ok();
        }

        [HttpGet]
        [Route("/api/[controller]/account/integrations")]
        public async Task<IActionResult> GetIntegrations([FromServices]OAuthPostgres oauthdb)
        {
            var username = "Guest";

            if (HttpContext.User.Identity.IsAuthenticated)
                username = HttpContext.User.Identity.Name;

            var integrations = await dropship.GetIntegrations(username, oauthdb);

            return Json(integrations);
        }

        [HttpPost]
        [Route("api/[controller]/account")]
        public async Task<IActionResult> CreateAccount([FromBody]DropshipAccountConfiguration config)
        {
            if (config.Subscription.EmptyOrNull())
                return NotFound();

            var username = "Guest";

            if (HttpContext.User.Identity.IsAuthenticated)
                username = HttpContext.User.Identity.Name;

            var account = await dropship.SetupAccount(config, username);

            HttpContext.Response.Headers.Add("X-USER-TOKEN", user.CreateJWT(new UserLoginModel() { Username = username }, account));

            return Ok();
        }
    }
}
