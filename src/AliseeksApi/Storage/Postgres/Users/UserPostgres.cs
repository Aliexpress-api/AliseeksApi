using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.User;
using Npgsql;
using Newtonsoft.Json;
using AliseeksApi.Exceptions.Postgres;
using AliseeksApi.Models.Search;

namespace AliseeksApi.Storage.Postgres.Users
{
    public class UsersPostgres : IUsersPostgres
    {
        IPostgresDb db;

        const string userInsertColumns = "username, password, salt, email, meta";
        const string userSelectColumns = "id, username, password, salt, created_date, email, meta";
        const string userSecureSelectColumns = "username, created_date, email, meta";
        const string userExistColumns = "username, email";
        const string userUpdateColumns = "password, salt, email, reset, meta";
        const string userTable = "users";
        
        const string savedSearchSelectColumns = "created, criteria, username, id";
        const string savedSearchTable = "savesearch";

        public UsersPostgres(IPostgresDb db)
        {
            this.db = db;
        }

        public async Task UpdateAsync(UserModel model)
        {
            var parameters = "@password, @salt, @email, @reset, @meta";
            var user = new UserModel();
            var command = new NpgsqlCommand();
            string[] columnUpdates = userUpdateColumns.Replace(" ", "").Split(',');
            string[] columnUpdatesParameters = parameters.Replace(" ", "").Split(',');
            var columnUpdateCommand = new string[columnUpdates.Length];

            for(int i = 0; i != columnUpdates.Length && i != columnUpdatesParameters.Length; i++)
            {
                columnUpdateCommand[i] = $"{columnUpdates[i]}={columnUpdatesParameters[i]}";
            }

            command.CommandText = $"UPDATE {userTable} SET {String.Join(",", columnUpdateCommand)} WHERE id=@id;";
            command.Parameters.AddWithValue("@password", model.Password ?? String.Empty);
            command.Parameters.AddWithValue("@salt", model.Salt ?? String.Empty);
            command.Parameters.AddWithValue("@email", model.Email ?? String.Empty);
            command.Parameters.AddWithValue("@reset", model.Reset ?? String.Empty);
            command.Parameters.AddWithValue("meta", NpgsqlTypes.NpgsqlDbType.Jsonb, (model.Meta == null) ? "" : JsonConvert.SerializeObject(model.Meta));
            command.Parameters.AddWithValue("@id", model.ID);

            await db.CommandNonqueryAsync(command);

            return;
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
            UserModel user = null;
            var command = new NpgsqlCommand();
            command.CommandText = $"SELECT {userSelectColumns} FROM {userTable} WHERE username=@username;";
            command.Parameters.AddWithValue("username", username);

            int ordinal = 0;

            await db.CommandReaderAsync(command, reader =>
            {
                user = new UserModel();
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

        public async Task<UserModel> FindByEmail(string email)
        {
            var user = new UserModel();
            var command = new NpgsqlCommand();
            command.CommandText = $"SELECT {userSelectColumns} FROM {userTable} WHERE email=@email;";
            command.Parameters.AddWithValue("email", email);

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

        public async Task<UserModel> FindByResetToken(string token)
        {
            var user = new UserModel();
            var command = new NpgsqlCommand();
            command.CommandText = $"SELECT {userSelectColumns} FROM {userTable} WHERE reset=@reset;";
            command.Parameters.AddWithValue("reset", token);

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

        public async Task<UserOverviewModel> GetUserOverview(string username)
        {
            UserOverviewModel overview = new UserOverviewModel();
            var command = new NpgsqlCommand();
            command.CommandText = $"SELECT {savedSearchSelectColumns} FROM {savedSearchTable} WHERE username=@username;";
            command.Parameters.AddWithValue("username", username);

            var savedSearches = new List<SavedSearchModel>();

            await db.CommandReaderAsync(command, reader =>
            {
                int ordinal = 0;
                var savedSearch = new SavedSearchModel();
                savedSearch.Created = reader.GetDateTime(ordinal++);
                savedSearch.Criteria = JsonConvert.DeserializeObject<SearchCriteria>(reader.GetString(ordinal++));
                savedSearch.Username = reader.GetString(ordinal++);
                savedSearch.ID = reader.GetInt32(ordinal++);

                savedSearches.Add(savedSearch);
            });

            overview.Username = username;
            overview.SavedSearches = savedSearches.ToArray();

            return overview;
        }
    }
}
