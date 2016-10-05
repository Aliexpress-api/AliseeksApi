using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Exceptions
{
    public class AllowedException : Exception
    {
        public AllowedException(string message)
            :base(message)
        {

        }

        public AllowedException(string message, Exception innerException)
            :base(message, innerException)
        {

        }
    }
}
