using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace ServerCore
{

    class Program
    {

        static Listener _listener = new Listener();

        static void OnAcceptHandler(Socket clientSocket)
        {
            try
            {
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
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            
        }
        static void Main(string[] args)
        {
            //DNS(Domain Name System) 사용
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

           
            _listener.Init(endPoint,OnAcceptHandler);

            while (true)
            {
                
            }
         
        }


    }
}