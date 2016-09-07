using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using AliseeksApi.Models.User;

namespace AliseeksApi.Services.User
{
    public interface IUserService
    {
        Task Register(UserNewModel model);
        Task<UserLoginResponse> Login(UserLoginModel model);
        Task Logout(UserLogoutModel model);
        Task ResetPassword(UserResetPasswordModel model);
    }
}
