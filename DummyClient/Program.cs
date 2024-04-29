using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DummyClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //DNS(Domain Name System) 사용
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            //휴대폰 설정
            Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);


            

            try
            {
                //문지기에게 입장문의
                socket.Connect(endPoint);
                Console.WriteLine($"Connected To{socket.RemoteEndPoint.ToString()}");

                //보낸다.
                byte[] sendbuff = Encoding.UTF8.GetBytes("Hello World");
                int sendBytes = socket.Send(sendbuff);
                //받는다.
                byte[] recvbuff = new byte[1024];
                int recvBytes = socket.Receive(recvbuff);
                string recvData = Encoding.UTF8.GetString(recvbuff, 0, recvBytes);

                Console.WriteLine($"[From Server] {recvData}");

                //나간다.
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }

            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}