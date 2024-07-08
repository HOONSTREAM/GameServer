using ServerCore;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DummyClient
{


    public class Packet
    {
        public ushort size;
        public ushort packetid;
    }

    class GameSession : Session
    {
        public override void OnConnected(EndPoint endpoint)
        {
            Console.WriteLine($"On Connected : {endpoint}");

            Packet packet = new Packet() { size = 4, packetid = 7 };

            for (int i = 0; i < 5; i++)
            {

                ArraySegment<byte> opensegment = SendBufferHelper.Open(4096);
                byte[] buffer = BitConverter.GetBytes(packet.size);
                byte[] buffer2 = BitConverter.GetBytes(packet.packetid);
                Array.Copy(buffer, 0, opensegment.Array, opensegment.Offset, buffer.Length);
                Array.Copy(buffer2, 0, opensegment.Array, opensegment.Offset + buffer.Length, buffer2.Length);

                ArraySegment<byte> sendbuff = SendBufferHelper.Close(packet.size);


                Send(sendbuff);
            }
        }
        public override void OnDisconnected(EndPoint endpoint)
        {
            Console.WriteLine($"OnDisconnected : {endpoint}");
        }
        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Server] : {recvData}");

            return buffer.Count;
        }
        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes : {numOfBytes}");
        }
    }

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

            connecter.Connect(endPoint, () => { return new GameSession(); });

            while (true)
            {
                try
                {
                }

                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                Thread.Sleep(100);
            }
        }

        }
    }