﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Models.User
{
    public class UserNewModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }

        public string PrimaryUse { get; set; }
    }
}
