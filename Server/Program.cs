using System;
using static System.Collections.Specialized.BitVector32;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using ServerCore;
using Server;


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


        _listener.Init(endPoint, () => { return new ClientSession(); }); // 무엇을만들어줄지만 지정

        while (true)
        {

        }

    }


}
