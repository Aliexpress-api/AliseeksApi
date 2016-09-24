using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Utility.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RedisIgnore : Attribute
    {
        public string Schema { get; set; }

        public RedisIgnore()
        {

        }

        public RedisIgnore(string schema)
        {
            this.Schema = schema;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RedisDefault : Attribute
    {
        public object Value { get; set; }
        public RedisDefault(object value)
        {
            this.Value = value;
        }
    }
}
