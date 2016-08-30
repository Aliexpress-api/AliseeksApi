using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Scheduling
{
    public class Scheduler : IScheduler
    {
        Queue<Action> action = new Queue<Action>();

        public void Enqueue(Action task)
        {
            action.Enqueue(task);
            runTask();
        }

        void runTask()
        {
            Task.Run(action.Dequeue());
        }
    }
}
