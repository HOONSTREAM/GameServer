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

        public struct SkillInfo
        {
            public int id;
            public short level;
            public float duration;

            public bool Write(Span<byte> s, ref ushort count) //여기서 Span은 전체 바이트 배열임. 두번째 인자는 실시간으로 우리가 몇번째 카운트를 작업하고 있는지.
            {
                bool success = true;
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), id);
                count += sizeof(int);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), level);
                count += sizeof(short);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), duration);
                count += sizeof(float);

                return true;
            }

            public void Read(ReadOnlySpan<byte> s, ref ushort count)
            {
                id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
                count += sizeof(int);
                level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
                count += sizeof(short);
                duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
                count += sizeof(float);

            }
        }
            public List<SkillInfo> skills = new List<SkillInfo>();
        

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

            //읽기 전용: ReadOnlySpan<T>는 데이터를 수정할 수 없도록 보장합니다. 이로 인해 데이터를 안전하게 읽을 수 있으며, 무결성이 유지됩니다.
            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array,segment.Offset,segment.Count);

           
            count += sizeof(ushort); 
            count += sizeof(ushort); 
            this.PlayerId = BitConverter.ToInt64(s.Slice(count, s.Length-count));

            count += sizeof(long); 

            ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length-count));
            count += sizeof(ushort);

            //바이트배열에서 스트링으로 역직렬화 해준다.
            this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
            count += nameLen;

            // skill List
            skills.Clear();
            ushort skillLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);

            for(int i = 0; i<skillLen; i++)
            {
                SkillInfo skill = new SkillInfo();
                skill.Read(s, ref count);

                skills.Add(skill);

            }

        }

        public override ArraySegment<byte> Write()
        {
            
            ArraySegment<byte> opensegment = SendBufferHelper.Open(4096); // 4096 바이트 크기의 버퍼를 연다. 패킷 데이터를 저장하기 위해 사용된다.

            bool success = true; // 직렬화 작업의 성공여부를 나타내는 변수.
            ushort count = 0; // 직렬화 한 바이트 수 

            // 버퍼의 배열, 오프셋, 카운트를 기반으로 Span<byte>를 생성한다. 
            //Span<T>는 포인터와 유사한 기능을 제공하면서도 메모리 안전성을 보장합니다.
            //이는 배열, 문자열, 또는 기타 메모리 블록의 부분을 참조할 수 있습니다.
            Span<byte> s = new Span<byte>(opensegment.Array, opensegment.Offset, opensegment.Count);

            count += sizeof(ushort); // 패킷 크기 필드를 건너뛰기 위해 count를 ushort만큼 증가시킨다.


            success &= BitConverter.TryWriteBytes(s.Slice(count,s.Length - count), this.packetid); // packetid를 버퍼에 쓰고, Bitconverter.Trywritebytes 메서드는 데이터를 지정된 위치에 쓰고, 성공여부를 반환한다.
            count += sizeof(ushort); // count를 packetid 필드 크기만큼 증가시킨다.
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.PlayerId);
            count += sizeof(long); // count를 Playerid 필드 크기만큼 증가시킨다.
         

            //======string======//
            // 이름 문자열을 유니코드 바이트 배열로 변환하여 버퍼에 쓴다. nameLen은 변환된 바이트 배열의 길이를 나타낸다.
            ushort nameLen =(ushort) Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, opensegment.Array, opensegment.Offset + count + sizeof(ushort));
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen); // 이름길이(nameLen)을 버퍼에 쓴다.

            count += sizeof(ushort); // count를 이름길이 필드 크기만큼 증가시킨다. (이름 길이를 저장하는 ushort 공간을 건너뛰기 위해)
            count += nameLen; // count를 이름 데이터 크기만큼 증가시킨다. (실제 이름 데이터를 건너뛰기 위해)


            //======Skill List======//
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)skills.Count);
            count += sizeof(ushort);

            foreach(SkillInfo skill in skills)
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
            packet.skills.Add(new PlayerInfoReq.SkillInfo() { id = 101, level = 1 ,duration = 3.0f});
            packet.skills.Add(new PlayerInfoReq.SkillInfo() { id = 201, level = 2, duration = 4.0f });
            packet.skills.Add(new PlayerInfoReq.SkillInfo() { id = 301, level = 3, duration = 5.0f });


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
