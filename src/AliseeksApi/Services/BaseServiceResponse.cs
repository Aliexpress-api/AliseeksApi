using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Services
{
    public class BaseServiceResponse
    {
        public int Code { get; set; }
        public string Message { get; set; }

        public BaseServiceResponse()
        {
            Code = 200;
        }
    }
}
