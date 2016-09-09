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
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using AliseeksApi.Utility.Security;
using Microsoft.AspNetCore.Mvc;
using AliseeksApi.Services.Email;
using AliseeksApi.Models.Email;

namespace AliseeksApi.Services.User
{
    public class UserService : IUserService
    {
        IJwtFactory jwtFactory;
        IUsersPostgres db;
        ISecurityHasher hasher;
        IEmailService email;

        public UserService(IJwtFactory jwtFactory, IUsersPostgres db, ISecurityHasher hasher,
            IEmailService email)
        {
            this.jwtFactory = jwtFactory;
            this.db = db;
            this.hasher = hasher;
            this.email = email;
        }

        public async Task<UserLoginResponse> Login(UserLoginModel model)
        {
            var userModel = await db.FindByUsername(model.Username);

            string hashedPassword = hasher.HashWithSalt(model.Password, userModel.Salt).Hash;

            if (hashedPassword != userModel.Password)
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
            await Task.Delay(10);
            //Invalidate a token
            //TODO: Invalidate a token
            return new BaseServiceResponse();
        }

        public async Task<BaseServiceResponse> Register(UserNewModel model)
        {
            var response = new BaseServiceResponse();

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

            var hash = hasher.Hash(userModel.Password);
            userModel.Password = hash.Hash;
            userModel.Salt = hash.Salt;

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
                //Should not get this exception
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

            var user = await db.FindByEmail(model.Email);

            if (user == null) { return response; }

            string resetToken = hasher.Hash(model.Email).Hash;
            user.Reset = resetToken;

            await db.UpdateAsync(user);

            var resetModel = new PasswordResetModel()
            {
                ActionUrl = $"localhost:5220/user/reset?token={resetToken}",
                Name = user.Username,
                SenderName = "Alex",
                Subject = "Password Reset",
                ToAddress = user.Email
            };

            await email.SendPasswordResetTo(resetModel);

            //TODO: Reset password
            return response;
        }

        public async Task<BaseServiceResponse> ResetValid(UserResetValidModel model)
        {
            var response = new BaseServiceResponse();

            var user = await db.FindByResetToken(model.Token);

            if (user == null) { return response; }

            var hashed = hasher.Hash(model.NewPassword);
            user.Password = hashed.Hash;
            user.Salt = hashed.Salt;

            await db.UpdateAsync(user);

            //TODO: Reset password
            return response;
        }

    }
}
