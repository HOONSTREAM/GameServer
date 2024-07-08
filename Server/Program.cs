using System;
using static System.Collections.Specialized.BitVector32;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using ServerCore;


public class Packet
{
    public ushort size;
    public ushort packetid;
}

class GameSession : PacketSession
{
    public override void OnConnected(EndPoint endpoint)
    {
        Console.WriteLine($"OnConnected : {endpoint}");
        //Packet packet = new Packet() { size = 100, packetid = 10 };

        //ArraySegment<byte> opensegment = SendBufferHelper.Open(4096);
        //byte[] buffer = BitConverter.GetBytes(packet.size);
        //byte[] buffer2 = BitConverter.GetBytes(packet.packetid);
        //Array.Copy(buffer, 0, opensegment.Array, opensegment.Offset, buffer.Length);
        //Array.Copy(buffer2, 0, opensegment.Array, opensegment.Offset + buffer.Length, buffer2.Length);

        //ArraySegment<byte> sendbuff = SendBufferHelper.Close(buffer.Length + buffer2.Length);

        //Send(sendbuff);
        Thread.Sleep(5000);
        Disconnect();
    }

    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + 2);
        Console.WriteLine($"RecvPacketId : {id}, RecvSize : {size}");
        
    }


    public override void OnDisconnected(EndPoint endpoint)
    {
        Console.WriteLine($"OnDisconnected : {endpoint}");
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


        _listener.Init(endPoint, () => { return new GameSession(); }); // 무엇을만들어줄지만 지정

        while (true)
        {

        }

    }


}
