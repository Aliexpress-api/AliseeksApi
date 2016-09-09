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
    public class AliSearchEncoder
    {
        public AliSearchEncoder()
        {

        }

        public string CreateQueryString(SearchCriteria criteria)
        {
            var keyvalue = new List<string>();

            PropertyInfo[] props = criteria.GetType().GetProperties();
            foreach(PropertyInfo prop in props)
            {
                QueryStringEncode encode = prop.GetCustomAttribute<QueryStringEncode>();
                object val = prop.GetValue(criteria);
                if(val != null && encode != null)
                {
                    var encodedValue = val.ToString().Replace(" ", "+");
                    if (val.GetType() == typeof(bool)) encodedValue = ((bool)val).YesOrNo();

                    keyvalue.Add($"{encode.Name}={encodedValue.ToString()}");
                }
            }

            string qs = string.Join("&", keyvalue.ToArray());

            return Uri.EscapeUriString(qs);
        }
    }
}
