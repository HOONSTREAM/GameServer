using ServerCore;
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

            Connector connecter = new Connector();

            connecter.Connect(endPoint, () => { return SessionManager.Instance.Generate(); } , 500);

            while (true)
            {
                try
                {
                    SessionManager.Instance.SendForEach();
                }

                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                Thread.Sleep(250);
            }
        }

    }

}