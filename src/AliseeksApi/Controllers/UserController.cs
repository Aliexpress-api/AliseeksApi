using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AliseeksApi.Models.User;
using AliseeksApi.Services.User;
using SharpRaven.Core;
using SharpRaven.Core.Data;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace AliseeksApi.Controllers
{
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        IUserService user;
        IRavenClient raven;

        public UserController(IUserService user, IRavenClient raven)
        {
            this.user = user;
            this.raven = raven;
        }

        [HttpPost]
        [Route("/api/[controller]/register")]
        public async Task<IActionResult> Register([FromBody]UserNewModel model)
        {
            var response = await user.Register(model);

            return StatusCode(response.Code, response.Message);
        }

        [HttpPost]
        [Route("/api/[controller]/login")]
        public async Task<IActionResult> Login([FromBody]UserLoginModel model)
        {
            var response = await user.Login(model);
            if (response.Token == null)
                return NotFound();

            HttpContext.Response.Headers.Add("X-USER-TOKEN", response.Token);

            return Ok();
        }

        [HttpPost]
        [Route("/api/[controller]/logout")]
        public async Task<IActionResult> Logout([FromBody]UserLogoutModel model)
        {
            await user.Logout(model);
            return Ok();
        }

        [HttpPost]
        [Route("/api/[controller]/reset")]
        public async Task<IActionResult> Reset([FromBody]UserResetPasswordModel model)
        {
            await user.ResetPassword(model);
            return Ok();
        }

        [HttpPost]
        [Route("/api/[controller]/resetvalid")]
        public async Task<IActionResult> ResetValid([FromBody]UserResetValidModel model)
        {
            //Replace spaces with '+' for encoding purposes, are there any other characters that may need to be changed?
            model.Token = model.Token.Replace(' ', '+');
            await user.ResetValid(model);
            return Ok();
        }

        [HttpGet]
        [Route("/api/[controller]/{username}")]
        [Authorize]
        public async Task<IActionResult> Overview([FromRoute] string username)
        {
            var response = await user.Overview(username);

            return Json(response);
        }
    }
}
