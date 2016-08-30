using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Utility.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = true)]
    public class QueryStringEncode : System.Attribute
    {
        public string Name { get; set; }
        
        public QueryStringEncode(string qsName)
        {
            this.Name = qsName;
        }
    }
}
