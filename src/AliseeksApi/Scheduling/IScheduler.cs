using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Scheduling
{
    public interface IScheduler
    {
        void Enqueue(Action task);
    }
}
