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
using Microsoft.Extensions.Logging;
using AliseeksApi.Utility.Extensions;

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
            if (model.Username == null || model.Password == null)
                return new UserLoginResponse();
            model.Username = model.Username.ToLower();

            var userModel = await db.FindByUsername(model.Username.ToLower());

            //Invalid username
            if (userModel == null)
                return new UserLoginResponse();

            //Hash the login password with user salt
            string hashedPassword = hasher.HashWithSalt(model.Password, userModel.Salt).Hash;

            //Verify if hashed password and login hashed password match
            if (hashedPassword != userModel.Password)
                return new UserLoginResponse();

            //Set security claims and encode into JWT
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.Name, model.Username.ToLower())
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
            if(model.Username == null || model.Password == null || model.Email == null) { return response; }
            model.Username = model.Username.ToLower();
            model.Email = model.Email.ToLower();

            //Convert to new User Model
            var userModel = new UserModel()
            {
                Username = model.Username.ToLower(),
                Password = model.Password,
                Email = model.Email.ToLower(),
                Meta = new UserMetaModel()
                {
                    PrimaryUse = model.PrimaryUse
                }
            };

            var hash = hasher.Hash(userModel.Password);
            userModel.Password = hash.Hash;
            userModel.Salt = hash.Salt;

            var exists = await db.Exists(userModel);
            if (exists != null && exists.Email == model.Email)
                return new BaseServiceResponse() { Code = 409, Message = "Email already in use" };
            else if (exists != null && exists.Username.ToLower() == model.Username.ToLower())
                return new BaseServiceResponse() { Code = 409, Message = "Username already in use" };

            try
            {
                await db.InsertAsync(userModel);
            }
            catch (PostgresDuplicateValueException e)
            {
                //Should not get this exception
                throw e;
            }
            catch (Exception e)
            {
                //Rethrow until we can find a better way to handle errors
                throw e;
            }

            AppTask.Forget(() =>  email.SendWelcomeTo(new WelcomeModel()
                {
                    Address = userModel.Email,
                    Subject = "Welcome to Aliseeks",
                    User = userModel.Username
                }));

            return response;
        }

        public async Task<BaseServiceResponse> ResetPassword(UserResetPasswordModel model)
        {
            var response = new BaseServiceResponse();
            if(model.Email == null) { return response; }

            var user = await db.FindByEmail(model.Email.ToLower());

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

            try
            {
                await db.UpdateAsync(user);
            }
            catch(Exception e)
            {
                //Rethrow until we can find a better way to handle errors
                throw e;
            }

            return response;
        }

    }
}
