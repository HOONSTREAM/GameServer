using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class Listener //논블록킹방식으로 만듬.
    {
        Socket _listenSocket;
        Func<Session> _sessionFactory; // 세션을 어떤방식으로 누구를 만들어줄지 결정

        public void Init(IPEndPoint endPoint, Func<Session> sessionFactory, int register = 10, int backlog = 100)
        {
            _listenSocket =  new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory += sessionFactory;

            //문지기교육
            _listenSocket.Bind(endPoint);

            //영업시작
            //backlog : 최대 대기수
            _listenSocket.Listen(backlog);


            /*SocketAsyncEventArgs 클래스는 .NET 프레임워크에서 네트워크 소켓 통신을 위해 
             * 비동기 작업을 수행할 때 사용되는 클래스입니다. 이 클래스는 효율적인 I/O 작업을 가능하게 해주며, 
             * 특히 많은 양의 네트워크 연결을 관리해야 하는 서버 애플리케이션에서 유용합니다.*/


            // 문지기를 10명으로 늘린다.
            for(int i = 0; i < register; i++)
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
                RegisterAccept(args);

            }
          
        
        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;

           bool pending = _listenSocket.AcceptAsync(args); // 바로 처리될수도 있고, 안될수도 있고...

            if(pending == false) 
            {
                OnAcceptCompleted(null, args);
            }
        }
        void OnAcceptCompleted(object sender , SocketAsyncEventArgs args)
        {
            if(args.SocketError == SocketError.Success)
            {
                //실제로 유저가 왔으면 ?
                Session session = _sessionFactory.Invoke();
                session.Start(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
                
            }

            else
            {
                Console.WriteLine(args.SocketError.ToString());
            }
              

            RegisterAccept(args);

        }
 
    }
}
