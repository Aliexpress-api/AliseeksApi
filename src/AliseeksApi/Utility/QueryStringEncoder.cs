using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Reflection;
using AliseeksApi.Models.Search;
using AliseeksApi.Utility.Attributes;
using AliseeksApi.Utility.Extensions;
using System.Net;

namespace AliseeksApi.Utility
{
    public class QueryStringEncoder
    {
        public QueryStringEncoder()
        {

        }

        public string CreateQueryString(SearchCriteria criteria, SearchService service)
        {
            var keyvalue = new List<string>();

            PropertyInfo[] props = criteria.GetType().GetProperties();
            foreach(PropertyInfo prop in props)
            {
                var encodes = prop.GetCustomAttributes<QueryStringEncode>();
                foreach(var encode in encodes)
                {
                    object val = prop.GetValue(criteria);
                    if (val != null && encode != null && encode.Service == service)
                    {
                        var encodedValue = val.ToString();

                        if(encode.Value != null)
                        {
                            encodedValue = encode.Value(val).ToString();
                        }

                        //var encodedValue = val.ToString().Replace(" ", "+");
                        //if (val.GetType() == typeof(bool)) encodedValue = ((bool)val).YesOrNo();

                        keyvalue.Add($"{encode.Name}={encodedValue.ToString()}");
                    }
                }
            }

            string qs = string.Join("&", keyvalue.ToArray());

            return Uri.EscapeUriString(qs);
        }
    }
}
