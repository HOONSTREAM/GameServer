using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{
    struct JobTimerelem : IComparable<JobTimerelem>
    {
        public int execTick; // 실행시간
        public Action action; // 갖고 있는 행위
        public int CompareTo(JobTimerelem other)
        {
            return other.execTick - execTick;
        }
    }
    internal class JobTimer
    {
        PriorityQueue<JobTimerelem> _pq = new PriorityQueue<JobTimerelem>();
        object _lock = new object();

        public static JobTimer Instance { get; } = new JobTimer();

        public void Push(Action action, int tickAfter = 0) // tickAfter : 몇 틱 후에 실행해야하는지 체크
        {
            JobTimerelem job;
            job.execTick = System.Environment.TickCount + tickAfter;
            job.action = action;

            lock (_lock)
            {
                _pq.Push(job);
            }
        }

        public void Flush()
        {
            while (true)
            {
                int now = System.Environment.TickCount;
                JobTimerelem job;

                lock (_lock)
                {
                    if (_pq.Count == 0) { break; }


                    job = _pq.Peek();

                    if (job.execTick > now)
                    {
                        break;
                    }

                    _pq.Pop();

                    job.action.Invoke();

                }
            }
        }
    }
}

