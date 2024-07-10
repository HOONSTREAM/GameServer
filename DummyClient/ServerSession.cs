using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Collections.Specialized;

namespace DummyClient
{
    public class Packet // 패킷 헤더
    {
        public ushort size;
        public ushort packetid;
    }

    class PlayerInfoReq : Packet
    {
        public long PlayerId;
    }

    class PlayerInfoOk : Packet
    {
        public int hp;
        public int attack;

    }

    public enum PacketID
    {
        PlayrInfoReq = 1,
        PlayrInfoOk = 2,
    }

    class ServerSession : Session
    {
        public override void OnConnected(EndPoint endpoint)
        {
            Console.WriteLine($"On Connected : {endpoint}");

            PlayerInfoReq packet = new PlayerInfoReq() { packetid = (ushort)PacketID.PlayrInfoReq, PlayerId = 1001 };

           // for (int i = 0; i < 5; i++)
            {

                ArraySegment<byte> opensegment = SendBufferHelper.Open(4096);

                bool success = true;
                ushort count = 0;

                count += 2;
                success &= BitConverter.TryWriteBytes(new Span<byte>(opensegment.Array, opensegment.Offset + count, opensegment.Count- count), packet.packetid);
                count += 2;
                success &= BitConverter.TryWriteBytes(new Span<byte>(opensegment.Array, opensegment.Offset+ count, opensegment.Count- count), packet.PlayerId);
                count += 8;
                success &= BitConverter.TryWriteBytes(new Span<byte>(opensegment.Array, opensegment.Offset, opensegment.Count), count);
                


                ArraySegment<byte> sendbuff = SendBufferHelper.Close(count);

                if (success)
                {
                    Send(sendbuff);
                }
               
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


}
