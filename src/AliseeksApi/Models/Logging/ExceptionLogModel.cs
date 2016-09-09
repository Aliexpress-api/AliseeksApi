using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Models.Logging
{
    public class ExceptionLogModel
    {
        public DateTime Date { get; set; }
        public int Criticality { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
    }
}
