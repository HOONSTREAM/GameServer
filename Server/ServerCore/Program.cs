namespace ServerCore
{

    //강의의핵심내용 : 디버그모드에서 코드를 설계 후 버그가 일어나지 않지만, 릴리즈모드로 가면서 컴파일러 최적화 때문에 버그가 발생하는 경우가 있다 .

    class Program
    {

        volatile static bool _stop = false; // 전역일때는 스레드가 공유하여 동시접근이 가능함.
        //volatile ; 코드상에서는 최적화 하지 말아주세요 키워드 , 최신값을 가져와라의 의미 

        static void ThreadMain()
        {
            Console.WriteLine("쓰레드 시작!");

            while(_stop == false)
            {

            }

            Console.WriteLine("쓰레드 종료!");
        }
       
        static void Main(string[] args)
        {
            Task t = new Task(ThreadMain);
            t.Start();

            Thread.Sleep(1000); // 밀리세컨드 단위

            _stop = true;

            Console.WriteLine("stop 호출");
            Console.WriteLine("종료 대기중");
            t.Wait(); // 스레드는 Join 
            Console.WriteLine("종료 성공");
        }
    }
}