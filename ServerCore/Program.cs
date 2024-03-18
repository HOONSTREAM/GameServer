namespace ServerCore
{
    // 24.03.09 spinlock 구현
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
                int original = Interlocked.CompareExchange(ref _locked, desired, expected); //_locked 와 expect(공간이 빔)를 비교해서 일치하면 desired(공간차지)로 updated하고 반복문탈출 
                if(original == expected)
                {
                    break;
                }

                // 만약 break가 안되면 쉬다오는 것이 context switching 
                Thread.Sleep(1); // 무조건 휴식 1ms 
                Thread.Sleep(0); // 조건부 양보 -> 나보다 우선순위가 낮은 애들한테는 양보불가 -> 우선순위가 나보다 같거나 높은쓰레드가 없으면 다시 본인에게 순서가 옴
                Thread.Yield(); // 관대한 양보 -> 관대하게 양보할테니, 지금 실행가능한 Thread가 있으면 실행하세요 -> 실행가능한 애가없으면 남은시간을 본인에게 소진함.



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