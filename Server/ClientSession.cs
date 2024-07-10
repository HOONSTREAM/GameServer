using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Packet
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
                case PacketID.PlayrInfoOk:
                    break;
                case PacketID.PlayrInfoReq:
                    {
                        long playerid = BitConverter.ToInt64(buffer.Array, buffer.Offset + count);
                        count += 8;

                        Console.WriteLine($"PlayerInfoReq : {playerid}");
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
