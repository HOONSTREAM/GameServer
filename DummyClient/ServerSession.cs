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
    public abstract class Packet // 패킷 헤더
    {
        public ushort size;
        public ushort packetid;

        public abstract ArraySegment<byte> Write();
        public abstract void Read(ArraySegment<byte> s);
    }

    class PlayerInfoReq : Packet
    {
        public long PlayerId;

        public PlayerInfoReq()
        {
            this.packetid = (ushort)PacketID.PlayrInfoReq;
        }

        public override void Read(ArraySegment<byte> s)
        {
            ushort count = 0;

           // ushort size = BitConverter.ToUInt16(s.Array, s.Offset);
            count += 2;
           // ushort id = BitConverter.ToUInt16(s.Array, s.Offset + count);
            count += 2;

            this.PlayerId = BitConverter.ToInt64(new ReadOnlySpan<byte>(s.Array, s.Offset+count, s.Count-count));

            count += 8;

        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> opensegment = SendBufferHelper.Open(4096);

            bool success = true;
            ushort count = 0;

            count += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(opensegment.Array, opensegment.Offset + count, opensegment.Count - count), this.packetid);
            count += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(opensegment.Array, opensegment.Offset + count, opensegment.Count - count), this.PlayerId);
            count += 8;
            success &= BitConverter.TryWriteBytes(new Span<byte>(opensegment.Array, opensegment.Offset, opensegment.Count), count);

            if(success == false)
            {
                return null;
            }

            return SendBufferHelper.Close(count);

        }
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

            PlayerInfoReq packet = new PlayerInfoReq() { PlayerId = 1001 };

            // for (int i = 0; i < 5; i++)
            {
                ArraySegment<byte> s = packet.Write();

                if(s != null)
                {
                    Send(s);
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
