using System;
using static System.Collections.Specialized.BitVector32;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using ServerCore;


public class Knight
{
    public int hp;
    public int attack;

}
class GameSession : Session
{
    public override void OnConnected(EndPoint endpoint)
    {
        Console.WriteLine($"OnConnected : {endpoint}");
        Knight knight = new Knight() { hp = 100, attack = 10 };

        ArraySegment<byte> opensegment = SendBufferHelper.Open(4096);
        byte[] buffer = BitConverter.GetBytes(knight.hp);
        byte[] buffer2 = BitConverter.GetBytes(knight.attack);
        Array.Copy(buffer, 0, opensegment.Array, opensegment.Offset, buffer.Length);
        Array.Copy(buffer2, 0, opensegment.Array, opensegment.Offset + buffer.Length, buffer2.Length);

        ArraySegment<byte> sendbuff = SendBufferHelper.Close(buffer.Length + buffer2.Length);

        Send(sendbuff);
        Thread.Sleep(1000);
        Disconnect();
    }
    public override void OnDisconnected(EndPoint endpoint)
    {
        Console.WriteLine($"OnDisconnected : {endpoint}");
    }
    public override int OnRecv(ArraySegment<byte> buffer)
    {
        string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
        Console.WriteLine($"[From Client] : {recvData}");

        return buffer.Count;
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
