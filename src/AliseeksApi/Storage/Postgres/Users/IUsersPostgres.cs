using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.User;

namespace AliseeksApi.Storage.Postgres.Users
{
    public interface IUsersPostgres
    {
        Task<UserModel> Exists(UserModel model);
        Task<UserModel> FindByUsername(string username);
        Task InsertAsync(UserModel model);
    }
}
