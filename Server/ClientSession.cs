using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server
{




    public enum PacketID
    {
        PlayerInfoReq = 1,
        Test = 2,
        PlayrInfoReq = 3,
    }

    class PlayerInfoReq
    {
        public byte testbyte;
        public long PlayerId;
        public string name;
        public class Skill
        {

            public int id;
            public short level;
            public float duration;
            public class Attribute
            {

                public int attri;



                public void Read(ReadOnlySpan<byte> s, ref ushort count)

                {
                    this.attri = BitConverter.ToInt32(s.Slice(count, s.Length - count));
                    count += sizeof(int);

                }

                public bool Write(Span<byte> s, ref ushort count) //여기서 Span은 전체 바이트 배열임. 두번째 인자는 실시간으로 우리가 몇번째 카운트를 작업하고 있는지.

                {
                    bool success = true;

                    success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.attri);
                    count += sizeof(int);

                    return true;
                }



            }


            public List<Attribute> attributes = new List<Attribute>();



            public void Read(ReadOnlySpan<byte> s, ref ushort count)

            {
                this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
                count += sizeof(int);
                this.level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
                count += sizeof(short);
                this.duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
                count += sizeof(float);
                attributes.Clear();
                ushort attributeLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
                count += sizeof(ushort);

                for (int i = 0; i < attributeLen; i++)
                {
                    Attribute attribute = new Attribute();
                    attribute.Read(s, ref count);

                    attributes.Add(attribute);

                }

            }

            public bool Write(Span<byte> s, ref ushort count) //여기서 Span은 전체 바이트 배열임. 두번째 인자는 실시간으로 우리가 몇번째 카운트를 작업하고 있는지.

            {
                bool success = true;

                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
                count += sizeof(int);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.level);
                count += sizeof(short);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.duration);
                count += sizeof(float);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.attributes.Count);
                count += sizeof(ushort);

                foreach (Attribute attribute in this.attributes)
                {
                    success &= attribute.Write(s, ref count);
                }

                return true;
            }



        }


        public List<Skill> skills = new List<Skill>();


        public void Read(ArraySegment<byte> segment)
        {
            ushort count = 0;

            //읽기 전용: ReadOnlySpan<T>는 데이터를 수정할 수 없도록 보장합니다. 이로 인해 데이터를 안전하게 읽을 수 있으며, 무결성이 유지됩니다.
            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
            count += sizeof(ushort);
            count += sizeof(ushort);

            this.testbyte = (byte)segment.Array[segment.Offset + count];
            count += sizeof(byte);
            this.PlayerId = BitConverter.ToInt64(s.Slice(count, s.Length - count));
            count += sizeof(long);
            ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);
            this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
            count += nameLen;
            skills.Clear();
            ushort skillLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);

            for (int i = 0; i < skillLen; i++)
            {
                Skill skill = new Skill();
                skill.Read(s, ref count);

                skills.Add(skill);

            }


        }

        public ArraySegment<byte> Write()
        {

            ArraySegment<byte> opensegment = SendBufferHelper.Open(4096); // 4096 바이트 크기의 버퍼를 연다. 패킷 데이터를 저장하기 위해 사용된다.

            bool success = true; // 직렬화 작업의 성공여부를 나타내는 변수.
            ushort count = 0; // 직렬화 한 바이트 수 

            // 버퍼의 배열, 오프셋, 카운트를 기반으로 Span<byte>를 생성한다. 
            //Span<T>는 포인터와 유사한 기능을 제공하면서도 메모리 안전성을 보장합니다.
            //이는 배열, 문자열, 또는 기타 메모리 블록의 부분을 참조할 수 있습니다.
            Span<byte> s = new Span<byte>(opensegment.Array, opensegment.Offset, opensegment.Count);

            count += sizeof(ushort); // 패킷 크기 필드를 건너뛰기 위해 count를 ushort만큼 증가시킨다.


            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.PlayerInfoReq); // packetid를 버퍼에 쓰고, Bitconverter.Trywritebytes 메서드는 데이터를 지정된 위치에 쓰고, 성공여부를 반환한다.
            count += sizeof(ushort); // count를 packetid 필드 크기만큼 증가시킨다.


            opensegment.Array[opensegment.Offset + count] = (byte)this.testbyte;
            count += sizeof(byte);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.PlayerId);
            count += sizeof(long);
            ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, opensegment.Array, opensegment.Offset + count + sizeof(ushort));
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen); // 이름길이(nameLen)을 버퍼에 쓴다.

            count += sizeof(ushort); // count를 이름길이 필드 크기만큼 증가시킨다. (이름 길이를 저장하는 ushort 공간을 건너뛰기 위해)
            count += nameLen; // count를 이름 데이터 크기만큼 증가시킨다. (실제 이름 데이터를 건너뛰기 위해)
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.skills.Count);
            count += sizeof(ushort);

            foreach (Skill skill in this.skills)
            {
                success &= skill.Write(s, ref count);
            }


            success &= BitConverter.TryWriteBytes(s, count); // 최종 패킷의 크기를 버퍼의 시작부분에 쓴다.

            if (success == false)
            {
                return null;
            }

            return SendBufferHelper.Close(count); // 직렬화 작업이 성공하면 버퍼를 닫고 직렬화된 데이터를 포함하는 어레이 세그먼트를 반환한다.

        }
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

                        foreach(PlayerInfoReq.Skill skill in p.skills)
                        {
                            Console.WriteLine($"Skill : {skill.id} , {skill.level}, {skill.duration}");
                        }

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
