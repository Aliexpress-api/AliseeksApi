﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Utility.Extensions
{
    public static class TypeExtensions
    {
        public static string YesOrNo(this bool value)
        {
            return value ? "y" : "n";
        }
    }
}