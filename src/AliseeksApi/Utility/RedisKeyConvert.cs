using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using AliseeksApi.Utility.Attributes;
using System.Text;

namespace AliseeksApi.Utility
{
    public class RedisKeyConvert
    {
        public static string Serialize(object model, string schema = null)
        {
            var builder = new List<string>();
            var propertyInfo = model.GetType().GetProperties();

            builder.Add(model.GetType().Name);

            foreach(var property in propertyInfo)
            {
                var ignore = property.GetCustomAttribute<RedisIgnore>();
                if (ignore != null && ignore.Schema == null)
                    continue;

                if (ignore != null && schema == ignore.Schema)
                    continue;

                var val = property.GetValue(model);
                var def = property.GetCustomAttribute<RedisDefault>();
                if (def != null && val == def.Value)
                    continue;
                if (val == null)
                    continue;

                builder.Add(val.ToString());
            }

            return String.Join(":", builder.ToArray());
        }
    }
}
