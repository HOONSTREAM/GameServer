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

                 Session session = new Session();
                session.Start(clientSocket);


                //보낸다.
                byte[] sendbuff = Encoding.UTF8.GetBytes("Welcome To MMORPG Server ! ");
                session.Send(sendbuff);

                Thread.Sleep(1000);
                session.Disconnect();


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