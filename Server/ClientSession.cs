using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server
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

            this.PlayerId = BitConverter.ToInt64(new ReadOnlySpan<byte>(s.Array, s.Offset + count, s.Count - count)); // 패킷조작 선별

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

            if (success == false)
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


    class ClientSession : PacketSession
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
            ushort count = 0;

            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;


            switch ((PacketID)id)
            {
                    
                case PacketID.PlayrInfoReq:
                    {
                        PlayerInfoReq p = new PlayerInfoReq();
                        p.Read(buffer);

                        Console.WriteLine($"PlayerInfoReq : {p.PlayerId}");
                    }
                    break;

            }

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

}
