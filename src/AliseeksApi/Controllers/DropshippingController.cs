using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SharpRaven.Core;
using AliseeksApi.Services.Dropshipping;
using AliseeksApi.Models.Search;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace AliseeksApi.Controllers
{
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

        [HttpGet]
        [Route("/api/[controller]/add")]
        public async Task<IActionResult> Add([FromQuery]SingleItemRequest item)
        {
            await dropship.DropshipProduct(item);

            return Ok();
        }
    }
}
