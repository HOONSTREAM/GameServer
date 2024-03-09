namespace ServerCore
{

    class SpinLock
    {
        volatile int _locked = 0; // volatile 가시성보장

        public void Acquire()
        {
            int expected = 0;
            int desired = 1;

            while (true)
            {
                //int original = Interlocked.Exchange(ref _locked, 1); // _locked 에 1을 넣어주는 기능을 하는데, 넣어주기 직전의 값을 original에 반환함. 즉 0이 반환되면 1을 대입한다는 소리 

                // if(original == 0) // 락이 걸려있지 않으면 차지해야함.
                // {
                //     break; 
                // }
                //CAS = Compare and Swap
                int original = Interlocked.CompareExchange(ref _locked, desired, expected); //_locked 와 expect를 비교해서 일치하면 desired로 updated함 
                if(original == expected)
                {
                    break;
                }

            }

           
        }

        public void Release()
        {
            _locked = 0;
        }

    }


    class Program
    {

        static int _num = 0;
        static SpinLock _lock = new SpinLock();

        static void Thread_1()
        {
            for(int i = 0; i< 100000; i++)
            {
                _lock.Acquire();
                _num++;
                _lock.Release();
            }
            
        }

        static void Thread_2()
        {

            for (int i = 0; i < 100000; i++)
            {
                _lock.Acquire();
                _num--;
                _lock.Release();
            }

        }

        static void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);
            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(_num);
        }


    }
}