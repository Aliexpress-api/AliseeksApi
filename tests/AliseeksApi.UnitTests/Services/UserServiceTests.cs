using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Authentication;
using AliseeksApi.Services.User;
using AliseeksApi.Services.Email;
using AliseeksApi.Configuration;
using AliseeksApi.Storage.Postgres.Users;
using AliseeksApi.Storage.Postgres;
using AliseeksApi.Utility.Security;
using AliseeksApi.Models.User;
using Moq;
using Xunit;
using AliseeksApi.UnitTests.Utility;

namespace AliseeksApi.UnitTests.Services
{
    public class UserServiceTests
    {
        UserService service;
        Mock<IUsersPostgres> dbMock;
        Mock<IEmailService> emailMock;
        AliseeksJwtAuthentication auth;
        SecurityHasher hasher;

        public UserServiceTests()
        {
            var jwtOptions = new JwtOptions() { SecretKey = "notsosecrettestkeyofcoursedontuse" };
            hasher = new SecurityHasher();
            dbMock = new Mock<IUsersPostgres>();
            emailMock = new Mock<IEmailService>();
            auth = new AliseeksJwtAuthentication(new Microsoft.Extensions.Options.OptionsWrapper<JwtOptions>(jwtOptions), new FakeRavenClient().Object);

            service = new UserService(auth, dbMock.Object, hasher, emailMock.Object, new FakeRavenClient().Object, null);
        }

        [Fact]
        public async Task UserCanLogin()
        {
            UserLoginModel login = new UserLoginModel()
            {
                Username = "wakawaka54",
                Password = "testpassword"
            };

            var hash = hasher.Hash(login.Password);

            UserModel model = new UserModel()
            {
                Username = "wakawaka54",
                Password = hash.Hash,
                Salt = hash.Salt
            };

            dbMock.Setup(x => x.FindByUsername(It.Is<string>(u => u == model.Username))).ReturnsAsync(model);

            var token = await service.Login(login);
            Assert.True(token.Token != "");
        }

        [Fact]
        public async Task InvalidPasswordCannotLogin()
        {
            UserLoginModel login = new UserLoginModel()
            {
                Username = "wakawaka54",
                Password = "testpassword"
            };

            var hash = hasher.Hash("wrongpassword");

            UserModel model = new UserModel()
            {
                Username = "wakawaka54",
                Password = hash.Hash,
                Salt = hash.Salt
            };

            dbMock.Setup(x => x.FindByUsername(It.Is<string>(u => u == model.Username))).ReturnsAsync(model);

            var token = await service.Login(login);
            Assert.True(token.Token == null);
        }

        [Fact]
        public async Task InvalidUsernameCannotLogin()
        {
            UserLoginModel login = new UserLoginModel()
            {
                Username = "wakawaka54",
                Password = "rightpassword"
            };

            var hash = hasher.Hash("rightpassword");

            UserModel model = new UserModel()
            {
                Username = "wakawaka",
                Password = hash.Hash,
                Salt = hash.Salt
            };

            dbMock.Setup(x => x.FindByUsername(It.Is<string>(u => u != model.Username))).ReturnsAsync(null);

            var token = await service.Login(login);
            Assert.True(token.Token == null);
        }
    }
}
