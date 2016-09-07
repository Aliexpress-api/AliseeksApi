using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AliseeksApi.Models.User;
using AliseeksApi.Services.User;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace AliseeksApi.Controllers
{
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        IUserService user;

        public UserController(IUserService user)
        {
            this.user = user;
        }

        [HttpPost]
        [Route("/api/[controller]/register")]
        public IActionResult Register([FromBody]UserNewModel model)
        {
            return Ok();
        }

        [HttpPost]
        [Route("/api/[controller]/login")]
        public async Task<UserLoginResponse> Login([FromBody]UserLoginModel model)
        {
            var response = await user.Login(model);
            return response;
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
    }
}
