using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public interface IJobQueue
    {
        void Push(Action job);
    }
    public class JobQueue : IJobQueue
    {
        Queue<Action> _jobqueue = new Queue<Action>(); // 내가 해야할 일 목록
        object _lock = new object();
        bool _flush = false;

        public void Push(Action job) // 맨 처음 밀어넣은 애가 실행까지 담당하는 구조
        {
            bool flush = false;

            lock (_lock)
            {
                _jobqueue.Enqueue(job);

                if(_flush == false) // 본인이 밀어넣은 사람의 1등이면
                {
                    flush = _flush = true;
                }
            }

            if (flush) 
            {
                Flush();
            }
        }

        private void Flush()
        {
            while (true)
            {
                Action action = Pop(); // 일감(액션)을 뽑아온다.
                if(action == null) { return; }

                action.Invoke(); // 일감을 실행시킨다. (ex. 람다식으로 정의한 것들..)
            }
        }

        Action Pop()
        {
            lock (_lock)
            {
                if(_jobqueue.Count == 0)
                {
                    _flush = false; // 볼일이 다 끝났으니까, 다음사람이 푸시를 할거면 너가 관리를 해라 .
                    return null;
                }

                return _jobqueue.Dequeue();
            }
        }

    }
}
