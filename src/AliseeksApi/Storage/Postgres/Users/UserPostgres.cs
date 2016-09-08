using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.User;
using Npgsql;
using Newtonsoft.Json;
using AliseeksApi.Exceptions.Postgres;

namespace AliseeksApi.Storage.Postgres.Users
{
    public class UsersPostgres : IUsersPostgres
    {
        IPostgresDb db;

        const string userInsertColumns = "username, password, salt, email, meta";
        const string userSelectColumns = "id, username, password, salt, created_date, email, meta";
        const string userExistColumns = "username, email";
        const string userTable = "users";

        public UsersPostgres(IPostgresDb db)
        {
            this.db = db;
        }

        public async Task<UserModel> Exists(UserModel model)
        {
            var user = new UserModel();
            var command = new NpgsqlCommand();
            command.CommandText = $"SELECT {userExistColumns} FROM {userTable} WHERE username=@username OR email=@email;";
            command.Parameters.AddWithValue("username", model.Username);
            command.Parameters.AddWithValue("email", model.Email);

            int ordinal = 0;

            await db.CommandReaderAsync(command, reader =>
            {
                user.Username = reader.GetString(ordinal++);
                user.Email = reader.GetString(ordinal++);
            });

            return user;
        }

        public async Task InsertAsync(UserModel model)
        {
            var parameters = "@username, @password, @salt, @email, @meta";
            var command = new NpgsqlCommand();
            command.CommandText = $@"INSERT INTO {userTable}
            ({userInsertColumns})
            VALUES ({parameters});";

            command.Parameters.AddWithValue("username", model.Username);
            command.Parameters.AddWithValue("password", model.Password);
            command.Parameters.AddWithValue("salt", model.Salt);
            command.Parameters.AddWithValue("email", model.Email);
            command.Parameters.AddWithValue("meta", NpgsqlTypes.NpgsqlDbType.Jsonb, (model.Meta == null) ? "" : JsonConvert.SerializeObject(model.Meta));

            try
            {
                await db.CommandNonqueryAsync(command);
            }
            catch(Npgsql.PostgresException e)
            {
                switch(e.SqlState)
                {
                    case "23505": //unique_violation
                        throw new PostgresDuplicateValueException(e);
                }
            }
        }

        public async Task<UserModel> FindByUsername(string username)
        {
            var user = new UserModel();
            var command = new NpgsqlCommand();
            command.CommandText = $"SELECT {userSelectColumns} FROM {userTable} WHERE username=@username;";
            command.Parameters.AddWithValue("username", username);

            int ordinal = 0;

            await db.CommandReaderAsync(command, reader =>
            {
                user.ID = reader.GetInt32(ordinal++);
                user.Username = reader.GetString(ordinal++);
                user.Password = reader.GetString(ordinal++);
                user.Salt = reader.GetString(ordinal++);
                user.CreatedDate = reader.GetDateTime(ordinal++);
                user.Email = reader.GetString(ordinal++);
                user.Meta = JsonConvert.DeserializeObject<UserMetaModel>(reader.GetString(ordinal++));
            });

            return user;
        }
    }
}
