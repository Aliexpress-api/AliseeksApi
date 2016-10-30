using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Models.User
{
    public class UserModel
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Salt { get; set; }
        public string Password { get; set; }
        public string Reset { get; set; }
        public DateTime CreatedDate { get; set; }

        //Survey response
        public UserMetaModel Meta { get; set; }
    }

    public class UserMetaModel
    {
        public string PrimaryUse { get; set; }
        public string Referral { get; set; }
    }
}
