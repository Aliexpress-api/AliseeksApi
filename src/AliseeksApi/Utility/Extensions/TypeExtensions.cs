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

        public static string OneOrZero(this bool value)
        {
            return value ? "1" : "0";
        }
        
        public static bool Consume(this bool value, bool consumed)
        {
            return consumed == true ? true : value;
        }

        public static string ExtractNumerical(this string value)
        {
            string ret = "";

            for(int i = 0; i != value.Length; i++)
            {
                if(char.IsNumber(value[i]))
                {
                    ret += value[i];
                }
            }

            return ret;
        }

        public static bool EmptyOrNull(this string value)
        {
            return (value == null) || value == String.Empty;
        }

        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach(var item in items)
            {
                action(item);
            }
        }
    }
}
