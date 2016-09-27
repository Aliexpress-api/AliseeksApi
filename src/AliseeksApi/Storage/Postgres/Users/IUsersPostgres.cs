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
        Task<UserModel> FindByResetToken(string token);
        Task<UserModel> FindByEmail(string email);
        Task InsertAsync(UserModel model);
        Task UpdateAsync(UserModel model);
        Task<UserOverviewModel> GetUserOverview(string username);
    }
}
