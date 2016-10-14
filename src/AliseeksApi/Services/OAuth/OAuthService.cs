using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Storage.Postgres.OAuth;
using AliseeksApi.Models.OAuth;
using Newtonsoft.Json;

namespace AliseeksApi.Services.OAuth
{
    public class OAuthService
    {
        private readonly OAuthPostgres db;

        public OAuthService(OAuthPostgres db)
        {
            this.db = db;
        }

        public async Task<T> RetrieveOAuth<T>(string username) where T : IOAuthModel
        {
            var def = default(T);
            var service = def.GetServiceName();

            //Get all registered integrations for user
            var oauths = await db.GetMultipleByUsername(username);

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(oauths.FirstOrDefault(x => x.Service == service)));
        }

        public async Task<OAuthAccountModel[]> RetrieveMultiple(string username)
        {
            var oauths = await db.GetMultipleByUsername(username);

            return oauths;
        }
    }
}
