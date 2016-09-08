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
        Task<BaseServiceResponse> Register(UserNewModel model);
        Task<UserLoginResponse> Login(UserLoginModel model);
        Task<BaseServiceResponse> Logout(UserLogoutModel model);
        Task<BaseServiceResponse> ResetPassword(UserResetPasswordModel model);
    }
}
