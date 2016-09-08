using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AliseeksApi.Models.User;
using System.Security.Claims;
using AliseeksApi.Authentication;
using AliseeksApi.Storage.Postgres.Users;
using AliseeksApi.Exceptions.Postgres;

namespace AliseeksApi.Services.User
{
    public class UserService : IUserService
    {
        IJwtFactory jwtFactory;
        IUsersPostgres db;

        public UserService(IJwtFactory jwtFactory, IUsersPostgres db)
        {
            this.jwtFactory = jwtFactory;
            this.db = db;
        }

        public async Task<UserLoginResponse> Login(UserLoginModel model)
        {
            //TODO: Add hashing and salting

            var userModel = await db.FindByUsername(model.Username);
            if (userModel.Password != model.Password)
                return new UserLoginResponse();

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

        public async Task<BaseServiceResponse> Logout(UserLogoutModel model)
        {
            //Invalidate a token
            //TODO: Invalidate a token
            return new BaseServiceResponse();
        }

        public async Task<BaseServiceResponse> Register(UserNewModel model)
        {
            var response = new BaseServiceResponse();

            //TODO: Add hashing and salting

            //Convert to new User Model
            var userModel = new UserModel()
            {
                Username = model.Username,
                Password = model.Password,
                Email = model.Email,
                Meta = new UserMetaModel()
                {
                    PrimaryUse = model.PrimaryUse
                }
            };

            var exists = await db.Exists(userModel);
            if (exists.Email == model.Email)
                return new BaseServiceResponse() { Code = 409, Message = "Email already in use" };
            else if (exists.Username == model.Username)
                return new BaseServiceResponse() { Code = 409, Message = "Username already in use" };

            try
            {
                await db.InsertAsync(userModel);
            }
            catch (PostgresDuplicateValueException e)
            {
                //
            }
            catch (Exception e)
            {
                //Unknown exception, panic
            }

            return response;
        }

        public async Task<BaseServiceResponse> ResetPassword(UserResetPasswordModel model)
        {
            var response = new BaseServiceResponse();

            //TODO: Reset password
            return response;
        }
    }
}
