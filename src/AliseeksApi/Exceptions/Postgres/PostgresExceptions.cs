using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Exceptions.Postgres
{
    public class PostgresDuplicateValueException : Exception
    {
        public PostgresDuplicateValueException(Exception innerException)
            :base("An object with this value already exists", innerException)
        {

        }
    }
}
