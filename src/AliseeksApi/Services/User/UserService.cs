using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AliseeksApi.Models.User;
using System.Security.Claims;
using AliseeksApi.Authentication;

namespace AliseeksApi.Services.User
{
    public class UserService : IUserService
    {
        IJwtFactory jwtFactory;

        public UserService(IJwtFactory jwtFactory)
        {
            this.jwtFactory = jwtFactory;
        }

        public async Task<UserLoginResponse> Login(UserLoginModel model)
        {
            //TODO: Verify user identity

            var claims = new Claim[]
            {
                new Claim(ClaimTypes.Name, model.Username)
            };

            var response = new UserLoginResponse()
            {
                Token = jwtFactory.GenerateToken(claims)
            };

            return response;
        }

        public async Task Logout(UserLogoutModel model)
        {
            //Invalidate a token
            //TODO: Invalidate a token
            return;
        }

        public async Task Register(UserNewModel model)
        {
            //TODO: Register User
            return;
        }

        public async Task ResetPassword(UserResetPasswordModel model)
        {
            //TODO: Reset password
            return;
        }
    }
}
