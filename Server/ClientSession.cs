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
        public string name;


        public PlayerInfoReq()
        {
            this.packetid = (ushort)PacketID.PlayrInfoReq;
        }

        public override void Read(ArraySegment<byte> segment)
        {
            ushort count = 0;

            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);


            count += sizeof(ushort);
            count += sizeof(ushort);
            this.PlayerId = BitConverter.ToInt64(s.Slice(count, s.Length - count));

            count += sizeof(long);

            ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);

            //바이트배열에서 스트링으로 역직렬화 해준다.
            this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));

        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> opensegment = SendBufferHelper.Open(4096);

            bool success = true;
            ushort count = 0;

            Span<byte> s = new Span<byte>(opensegment.Array, opensegment.Offset, opensegment.Count);

            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.packetid);
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.PlayerId);
            count += sizeof(long);

            ushort nameLen = (ushort)Encoding.Unicode.GetByteCount(this.name); //바이트배열로 변환이 되었을 때의 길이

            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);

            count += sizeof(ushort);

            Encoding.Unicode.GetBytes(this.name);
            Array.Copy(Encoding.Unicode.GetBytes(this.name), 0, opensegment.Array, count, nameLen);
            count += nameLen;

            success &= BitConverter.TryWriteBytes(s, count);

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
                        Console.WriteLine($"Playername : {p.name}");

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
