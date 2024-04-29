using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace ServerCore
{

    class Program
    {

        
        static void Main(string[] args)
        {
            //DNS(Domain Name System) 사용
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            //문지기가 들고있는 휴대폰
            Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp); 

            //문지기교육
            listenSocket.Bind(endPoint);

            //영업시작
            //backlog : 최대 대기수
            listenSocket.Listen(10);

            try 
            {
                while (true)
                {
                    Console.WriteLine("Listening ...");

                    // 손님을 입장시킨다.
                    Socket clientSocket = listenSocket.Accept();

                    //받는다.
                    byte[] recvbuff = new byte[1024];
                    int recvBytes = clientSocket.Receive(recvbuff);
                    string recvData = Encoding.UTF8.GetString(recvbuff, 0, recvBytes);
                    Console.WriteLine($"[From Client] : {recvData}");


                    //보낸다.
                    byte[] sendbuff = Encoding.UTF8.GetBytes("Welcome To MMORPG Server ! ");
                    clientSocket.Send(sendbuff);

                    //쫓아낸다.
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }

            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
           

        }


    }
}