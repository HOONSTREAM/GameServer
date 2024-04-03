using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;


/*이 클래스는 C#에서 멀티스레드 환경에서 공유 자원에 대한 접근을 동기화하기 위한 커스텀 락(lock) 구현입니다. 이 락은 스핀락(Spinlock) 방식을 사용하여, 특정 조건이 만족될 때까지 현재 스레드를 바쁘게 대기시킵니다. 스핀락은 CPU 자원을 계속 사용하므로, 잠금을 기다리는 시간이 짧을 것으로 예상될 때 주로 사용됩니다.

이 락 구현은 읽기 잠금(Read Lock)과 쓰기 잠금(Write Lock)을 모두 지원합니다. 이는 독자-작가 잠금(Reader-Writer Lock)의 한 형태로 볼 수 있으며, 여러 스레드가 동시에 읽기 작업을 할 수 있지만 쓰기 작업은 단일 스레드에서만 수행할 수 있도록 합니다.

구현 세부사항
변수 설명

_flag: 잠금 상태를 나타내는 변수입니다. 32비트 중 상위 15비트는 쓰기 잠금을 위한 스레드 ID, 하위 16비트는 읽기 잠금 카운트를 나타냅니다.
writeCount: 현재 쓰기 잠금을 얻은 횟수입니다. 재진입 가능한 쓰기 잠금을 지원합니다.
EMPTY_FLAG, WRITE_MASK, READ_MASK, MAX_SPIN_COUNT: 각각 초기 상태 플래그, 쓰기 마스크, 읽기 마스크, 최대 스핀 카운트를 정의합니다.
메서드 설명

WriteLock: 쓰기 잠금을 시도합니다. 현재 스레드가 이미 쓰기 잠금을 가지고 있으면 writeCount를 증가시킵니다. 그렇지 않으면 아무도 잠금을 가지고 있지 않을 때까지 스핀합니다.
WriteUnLock: 쓰기 잠금을 해제합니다. writeCount를 감소시키고, 0이 되면 잠금 상태를 초기화합니다.
ReadLock: 읽기 잠금을 시도합니다. 현재 스레드가 쓰기 잠금을 가지고 있으면 단순히 _flag를 증가시킵니다. 그렇지 않으면, 아무도 쓰기 잠금을 가지고 있지 않을 때까지 읽기 카운트를 증가시킵니다.
ReadUnLock: 읽기 잠금을 해제합니다. _flag에서 읽기 카운트를 감소시킵니다.
동작 원리
WriteLock과 WriteUnLock은 쓰기 작업을 독점적으로 수행하기 위해 사용됩니다. 재진입이 가능하여, 같은 스레드에서 여러 번 잠금을 요청할 경우 writeCount를 통해 관리됩니다.
ReadLock과 ReadUnLock은 여러 스레드가 동시에 읽기 작업을 수행할 수 있도록 합니다. 하지만 쓰기 잠금이 활성화된 경우 읽기 잠금을 얻을 수 없습니다.
이 구현은 스핀락을 사용하므로, 잠금 대기 시간이 짧을 것으로 예상되는 상황에서 사용하기 적합합니다. 대기 시간이 길 경우 CPU 자원을 낭비할 수 있습니다.*/

namespace ServerCore
{
    // 재귀 락을 허용할 것인지 (No)
    // 스핀 락 정책 : (5000번 -> yield)

    class Lock
    {
        const int EMPTY_FLAG = 0x00000000;
        const int WRITE_MASK = 0x7FFF0000;
        const int READ_MASK = 0x0000FFFF;
        const int MAX_SPIN_COUNT = 5000;

        // [Unused(1)] [writeThreadID(15)] [ReadCount(16)]
        int _flag = EMPTY_FLAG;
        int writeCount = 0;

        public void WriteLock()
        {
            // 동일 쓰레드가 WriteLock을 이미 획득하고 있는지 확인
            int lockThreadID = (_flag & WRITE_MASK) >> 16;
            if(Thread.CurrentThread.ManagedThreadId == lockThreadID)
            {
                writeCount++;
                return;
            }
            // 아무도 WriteLock or ReadLock을 획득하고 있지 않을 때, 경합해서 소유권을 얻는다.
            int desired = (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;
            while (true)
            {
                for(int i =0; i < MAX_SPIN_COUNT; i++)
                {
                    if (Interlocked.CompareExchange(ref _flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                    {
                        writeCount = 1;
                        return;
                    }
                       
                   
                }

                Thread.Yield();
            }
        }

        public void WriteUnLock()
        {
            int lockCount = --writeCount;

            if(lockCount == 0)
            {
                Interlocked.Exchange(ref _flag, EMPTY_FLAG);
            }
           
        }

        public void ReadLock() //여러 스레드가 ReadLock을 잡을 수 있다.
        {
            // 동일 쓰레드가 WriteLock을 이미 획득하고 있는지 확인
            int lockThreadID = (_flag & WRITE_MASK) >> 16;
            if (Thread.CurrentThread.ManagedThreadId == lockThreadID)
            {
                Interlocked.Increment(ref _flag);
                return;
            }

            //아무도 WirteLock을 획득하고 있지않으면, ReadCount를 1 늘린다.
            while (true)
            {
                for(int i = 0; i<MAX_SPIN_COUNT; i++)
                {
                    int expected = (_flag & READ_MASK);

                    if (Interlocked.CompareExchange(ref _flag, expected + 1, expected) == expected)
                        return;
                    //if((_flag & WRITE_MASK) == 0) // 아무도 락을 획득하고 있지 않으면
                    //{
                    //    _flag = _flag + 1;
                    //    return;
                    //}
                }

                Thread.Yield(); // 5000번을 스핀해도 안되면 양보
            }
        }

        public void ReadUnLock()
        {
            Interlocked.Decrement(ref _flag);
        }
    }
}
