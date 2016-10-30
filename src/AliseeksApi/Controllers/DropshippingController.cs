using System;
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
using AliseeksApi.Storage.Postgres.Dropshipping;
using AliseeksApi.Services.Search;
using AliseeksApi.Services.OAuth;
using AliseeksApi.Models.OAuth;
using AliseeksApi.Utility;

namespace AliseeksApi.Controllers
{
    [Authorize]
    public class DropshippingController : Controller
    {
        private readonly DropshippingService dropship;
        private readonly IRavenClient raven;
        private readonly IUserService user;
        private readonly ShopifyService shopify;
        private readonly DropshipItemsPostgres dbItems;
        private readonly DropshipAccountsPostgres dbAccounts;
        private readonly ISearchService search;
        private readonly OAuthService oauthdb;


        public DropshippingController(DropshippingService dropship, ISearchService search, ShopifyService shopify,
            OAuthService oauthdb, DropshipItemsPostgres dbItems, DropshipAccountsPostgres dbAccounts, IRavenClient raven)
        {
            this.dropship = dropship;
            this.raven = raven;
            this.shopify = shopify;
            this.oauthdb = oauthdb;
            this.dbItems = dbItems;
            this.dbAccounts = dbAccounts;
            this.search = search;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return NotFound();
        }

        //Add product by SingleItemRequest
        [HttpPost]
        [Route("/api/[controller]/add")]
        public async Task<IActionResult> Add([FromBody]SingleItemRequest item)
        {
            var username = "Guest";

            if (HttpContext.User.Identity.IsAuthenticated)
                username = HttpContext.User.Identity.Name;

            if (item.Link.EmptyOrNull() && !item.Title.EmptyOrNull() && !item.ID.EmptyOrNull())
                item.Link = SearchEndpoints.AliexpressItemUrl(item.Title, item.ID);

            var detail = await search.ItemSearch(item);

            //Get integration access tokens
            var oauth = await oauthdb.RetrieveOAuth<OAuthShopifyModel>(username);
            if (oauth == null)
                return NotFound("No dropshipping integration setup for user");

            //Create the dropship item and model
            var dropshipItem = new DropshipItem()
            {
                Dropshipping = new DropshipItemModel()
                {
                    Source = item,
                    Username = username,
                    Rules = DropshipListingRules.Default,
                    OAuthID = oauth.ID
                },
                Product = new ShopifyProductModel()
                {
                    BodyHtml = detail.Description,
                    Title = detail.Name.Replace("/", "-"), //Fix slash in name issue

                    Variants = new List<ShopifyVarant>()
                    {
                        new ShopifyVarant()
                        {
                            InventoryPolicy = InventoryPolicy.Deny,
                            InventoryManagement = InventoryManagement.Shopify,
                            RequiresShipping = true,
                            Taxable = true
                        }
                    }
                }
            };

            //Add images from shopify
            var images = new List<ShopifyImageType>();
            foreach (var image in detail.ImageUrls)
            {
                images.Add(new ShopifyImageType()
                {
                    Src = image
                });
            }

            //Set the first image to the main dropship model image
            if (images.Count > 0)
                dropshipItem.Dropshipping.Image = images[0].Src;

            dropshipItem.Product.Images = images.ToArray();

            //Apply dropshipping rules
            dropshipItem.Dropshipping.Rules.ApplyRules(detail, dropshipItem.Product);

            //Add product
            var product = await shopify.AddProduct(username, dropshipItem.Product, oauth);

            dropshipItem.Dropshipping.ListingID = product.ID;

            await dbItems.InsertItem(dropshipItem.Dropshipping);

            return Ok();
        }

        //Update product listing
        [HttpPut]
        [Route("/api/[controller]/update")]
        public async Task<IActionResult> Update([FromBody]DropshipItemModel model)
        {
            //Get username from jwt
            var username = HttpContext.User.Identity.Name;

            model.Username = username;

            //Get integration information
            var oauth = await oauthdb.RetrieveOAuth<OAuthShopifyModel>(username);
            if (oauth == null)
                return NotFound("Integration not configured properly");

            //Get aliexpress item details
            var sourceItem = await search.ItemSearch(model.Source);

            if (sourceItem == null)
                return NotFound("Aliexpress source is incorrect");

            //Get shpoify item details
            var shopifyProducts = await shopify.GetProductsByID(username, new string[] { model.ListingID }, oauth);

            var product = shopifyProducts.FirstOrDefault();

            if (product == null)
                return NotFound("Shopify listing is incorrect");

            //Apply rules to shopify item
            if(model.Rules.ApplyRules(sourceItem, product))
            {
                //Update shopify item with rule results
                var updatedProduct = await shopify.UpdateProduct(username, product, oauth);
            }

            //Save the model in the DB
            await dbItems.UpdateRules(model);

            return Ok();
        }

        //Add to integration
        [HttpGet]
        [Route("/api/[controller]/{itemid}/add")]
        public async Task<IActionResult> AddToIntegration(int itemid)
        {
            var model = await dbItems.GetOneByID(itemid);

            var username = HttpContext.User.Identity.Name;

            model.Username = username;

            var oauth = await oauthdb.RetrieveOAuth<OAuthShopifyModel>(username);
            if (oauth == null)
                return NotFound("No integrations");

            var detail = await search.ItemSearch(model.Source);

            if (detail == null)
                return NotFound("Aliexpress source is incorrect");

            //Create the dropship item and model
            var dropshipItem = new DropshipItem()
            {
                Dropshipping = model,
                Product = new ShopifyProductModel()
                {
                    BodyHtml = detail.Description,
                    Title = detail.Name.Replace("/", "-"), //Fix slash in name issue

                    Variants = new List<ShopifyVarant>()
                    {
                        new ShopifyVarant()
                        {
                            InventoryPolicy = InventoryPolicy.Deny,
                            InventoryManagement = InventoryManagement.Shopify,
                            RequiresShipping = true,
                            Taxable = true
                        }
                    }
                }
            };

            //Add images from shopify
            var images = new List<ShopifyImageType>();
            foreach (var image in detail.ImageUrls)
            {
                images.Add(new ShopifyImageType()
                {
                    Src = image
                });
            }

            //Set the first image to the main dropship model image
            if (images.Count > 0)
                dropshipItem.Dropshipping.Image = images[0].Src;

            dropshipItem.Product.Images = images.ToArray();

            //Apply dropshipping rules
            dropshipItem.Dropshipping.Rules.ApplyRules(detail, dropshipItem.Product);

            //Add product
            var product = await shopify.AddProduct(username, dropshipItem.Product, oauth);

            dropshipItem.Dropshipping.ListingID = product.ID;

            await dbItems.UpdateListing(dropshipItem.Dropshipping);

            return Ok();
        }

        //Get products
        [HttpGet]
        [Route("/api/[controller]")]
        public async Task<IActionResult> Get(int limit = 50, int offset = 0)
        {
            //Get jwt username
            var username = HttpContext.User.Identity.Name;
           
            //Create return object
            var dropshipItems = new List<DropshipItem>();

            //Retrieve Dropship Item Models from db
            var items = await dbItems.GetMultipleByUsername(username);

            //Get integration oauth info
            var oauth = await oauthdb.RetrieveOAuth<OAuthShopifyModel>(username);

            //Add to return object
            items.ForEach(item => dropshipItems.Add(new DropshipItem()
            {
                Dropshipping = item
            }));

            var count = items.Length;

            if (items.Length != 0)
            {
                items = items.Skip(offset).Take(limit).ToArray();
                 
                //Put all item ids 
                var ids = new List<string>();
                items.ForEach(item => ids.Add(item.ListingID));

                var shopifyItems = await shopify.GetProductsByID(username, ids.ToArray(), oauth);
                shopifyItems.ForEach(shopifyItem =>
                {
                    var item = dropshipItems.FirstOrDefault(dropshipItem => dropshipItem.Dropshipping.ListingID == shopifyItem.ID);
                    if (item != null)
                        item.Product = shopifyItem;
                });
            }

            //Add total product count
            HttpContext.Response.Headers.Add(ApiConstants.HeaderItemCount, count.ToString());

            return Json(dropshipItems);
        }

        //Get product
        [HttpGet]
        [Route("/api/[controller]/{itemid}")]
        public async Task<IActionResult> GetProduct(int itemid)
        {
            var username = HttpContext.User.Identity.Name;

            var oauth = await oauthdb.RetrieveOAuth<OAuthShopifyModel>(username);

            var item = await dbItems.GetOneByID(itemid);

            if(item.Username != username)
                return NotFound("No item was found");

            var shopifyItems = await shopify.GetProductsByID(username, new string[] { item.ListingID }, oauth);

            var dropshipItem = new DropshipItem()
            {
                Dropshipping = item,
                Product = shopifyItems.FirstOrDefault()
            };

            return Json(dropshipItem);
        }

        [HttpDelete]
        [Route("/api/[controller]/{itemid}")]
        public async Task<IActionResult> DeleteItem(int itemid)
        {
            var username = HttpContext.User.Identity.Name;

            await dbItems.DeleteItem(itemid, username);

            return Ok();
        }

        //Obsolete method
 /*       //Get listing item by Integration Name and ItemID
        [HttpGet]
        [Route("/api/[controller]/integrations/{source}/{itemid}")]
        public async Task<IActionResult> GetListingItem(string source, string itemid)
        {
            var username = HttpContext.User.Identity.Name;

            var oauth = await oauthdb.RetrieveOAuth<OAuthShopifyModel>(username);
            if (oauth == null)
                return NotFound();

            var shopifyItems = await shopify.GetProductsByID(username, new string[] { itemid }, oauth);
            var shopifyItem = shopifyItems.FirstOrDefault(x => x.ID == itemid);

            if (shopifyItem == null)
                return NotFound();

            return Json(shopifyItem);
        }
*/
        //Get Account Overview
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

        //Create Account
        [HttpPost]
        [Route("api/[controller]/account")]
        public async Task<IActionResult> CreateAccount([FromBody]DropshipAccountConfiguration config, [FromServices]IUserService user)
        {
            if (config.Subscription.EmptyOrNull())
                return NotFound();

            var username = "Guest";

            if (HttpContext.User.Identity.IsAuthenticated)
                username = HttpContext.User.Identity.Name;

            var account = await dropship.SetupAccount(config, username);

            HttpContext.Response.Headers.Add("X-USER-TOKEN", user.CreateJWT(new UserLoginModel() { Username = username }, account)); //Update user JWT with dropshipping authentication

            return Ok();
        }

        //Get Account Info
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

        //Get Orders
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

        //Shopify OAuth Integration Request
        //Returns the Shopify OAuth Web Address for authenticating with Shopify
        [HttpGet]
        [Route("/api/[controller]/account/shopify/oauth")]
        public IActionResult GetShopifyOAuth(string shop, string redirect, [FromServices]ShopifyOAuth oauth)
        {
            var request = oauth.GetOAuthRequest(shop, redirect);

            return Json(request);
        }

        
        //Adds a shopify integration to a user
        //Verifys shopify oauth response
        [HttpPost]
        [Route("/api/[controller]/account/shopify/oauth")]
        public async Task<IActionResult> CreateShopifyOAuth([FromBody]ShopifyOAuthResponse response, [FromServices]ShopifyOAuth oauth)
        {
            if (!oauth.VerifyOAuthRequest(response))
                return NotFound();

            var username = String.Empty;

            if (HttpContext.User.Identity.IsAuthenticated)
                username = HttpContext.User.Identity.Name;

            var account = await dbAccounts.GetOneByUsername(username);

            if (!await shopify.AddShopifyIntegration(account, response, oauth))
                return NotFound();

            return Ok();
        }

        //Get Integrations
        [HttpGet]
        [Route("/api/[controller]/account/integrations")]
        public async Task<IActionResult> GetIntegrations([FromServices]OAuthPostgres oauthdb)
        {
            var username = "Guest";

            if (HttpContext.User.Identity.IsAuthenticated)
                username = HttpContext.User.Identity.Name;

            var integrations = await dropship.GetIntegrations(username);

            return Json(integrations);
        }

        //Delete Integration
        [HttpDelete]
        [Route("/api/[controller]/account/integrations/{id}")]
        public async Task<IActionResult> DeleteIntegration(int id, [FromServices]OAuthPostgres oauthdb)
        {
            var username = HttpContext.User.Identity.Name;

            await oauthdb.DeleteByID(id, username);

            return Ok();
        }
    }
}
