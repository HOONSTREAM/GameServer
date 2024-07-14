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
        public string name;


        public PlayerInfoReq()
        {
            this.packetid = (ushort)PacketID.PlayrInfoReq;
        }

        public override void Read(ArraySegment<byte> segment)
        {
            ushort count = 0;

            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array,segment.Offset,segment.Count);

           
            count += sizeof(ushort); 
            count += sizeof(ushort); 
            this.PlayerId = BitConverter.ToInt64(s.Slice(count, s.Length-count));

            count += sizeof(long); 

            ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length-count));
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
            success &= BitConverter.TryWriteBytes(s.Slice(count,s.Length - count), this.packetid);
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.PlayerId);
            count += sizeof(long);
         

            ushort nameLen =(ushort) Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, opensegment.Array, opensegment.Offset + count + sizeof(ushort));
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);

            count += sizeof(ushort);
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

    class ServerSession : Session
    {
        public override void OnConnected(EndPoint endpoint)
        {
            Console.WriteLine($"On Connected : {endpoint}");

            PlayerInfoReq packet = new PlayerInfoReq() { PlayerId = 1001, name = "ABCD" };

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
