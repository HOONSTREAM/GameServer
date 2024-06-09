using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace ServerCore
{
    class GameSession : Session
    {
        public override void OnConnected(EndPoint endpoint)
        {
            byte[] sendbuff = Encoding.UTF8.GetBytes("Welcome To MMORPG Server ! ");
            Send(sendbuff);
            Thread.Sleep(1000);
            Disconnect();
        }
        public override void OnDisconnected(EndPoint endpoint)
        {
            Console.WriteLine($"OnDisconnected : {endpoint}");
        }
        public override void OnRecv(ArraySegment<byte> buffer)
        {
           string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
           Console.WriteLine($"[From Client] : {recvData}");
        }
        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes : {numOfBytes}");
        }
    }

    class Program
    {

        static Listener _listener = new Listener();

     
        static void Main(string[] args)
        {
            //DNS(Domain Name System) 사용
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

           
            _listener.Init(endPoint,() => { return new GameSession(); }); // 무엇을만들어줄지만 지정

            while (true)
            {
                
            }
         
        }


    }
}