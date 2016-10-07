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

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace AliseeksApi.Controllers
{
    //[Authorize]
    public class DropshippingController : Controller
    {
        private readonly DropshippingService dropship;
        private readonly IRavenClient raven;

        public DropshippingController(DropshippingService dropship, IRavenClient raven)
        {
            this.dropship = dropship;
            this.raven = raven;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("/api/[controller]/add")]
        public async Task<IActionResult> Add([FromQuery]SingleItemRequest item)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
                item.Username = HttpContext.User.Identity.Name;
            else
                item.Username = "Guest";

            await dropship.AddProduct(item);

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

        [HttpPost]
        [Route("api/[controller]/account")]
        public async Task<IActionResult> CreateAccount()
        {
            return Ok();
        }
    }
}
