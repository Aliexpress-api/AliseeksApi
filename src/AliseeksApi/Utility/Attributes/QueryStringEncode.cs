using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Utility.Attributes
{
    public enum SearchService
    {
        Aliexpress, DHGate
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = true)]
    public class QueryStringEncode : System.Attribute
    {
        public string Name { get; set; }
        public SearchService Service { get; set; }
        public Func<object, object> Value { get; set; }
        
        public QueryStringEncode(string qsName)
        {
            this.Name = qsName;
        }

        public QueryStringEncode(SearchService service, string qsName)
        {
            this.Name = qsName;
            this.Service = service;
        }

        public QueryStringEncode(SearchService service, string qsName, Func<object, object> value)
        {
            this.Service = service;
            this.Name = qsName;
            this.Value = value;
        }
    }
}
