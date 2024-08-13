using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Collections.Specialized;


public enum PacketID
{
    C_Chat = 1,
	S_Chat = 2,
	
}

interface IPacket
{
	ushort Protocol { get; }
	void Read(ArraySegment<byte> segment);
	ArraySegment<byte> Write();

}



class C_Chat : IPacket
{
    public string chat;
    
    public ushort Protocol { get { return (ushort)PacketID.C_Chat; } }
  
    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;

        //읽기 전용: ReadOnlySpan<T>는 데이터를 수정할 수 없도록 보장합니다. 이로 인해 데이터를 안전하게 읽을 수 있으며, 무결성이 유지됩니다.
        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array,segment.Offset,segment.Count);   
        count += sizeof(ushort); 
        count += sizeof(ushort); 

        ushort chatLen = BitConverter.ToUInt16(s.Slice(count, s.Length-count));
		count += sizeof(ushort);
		this.chat = Encoding.Unicode.GetString(s.Slice(count, chatLen));
		count += chatLen;


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


        success &= BitConverter.TryWriteBytes(s.Slice(count,s.Length - count), (ushort)PacketID.C_Chat); // packetid를 버퍼에 쓰고, Bitconverter.Trywritebytes 메서드는 데이터를 지정된 위치에 쓰고, 성공여부를 반환한다.
        count += sizeof(ushort); // count를 packetid 필드 크기만큼 증가시킨다.


         ushort chatLen =(ushort) Encoding.Unicode.GetBytes(this.chat, 0, this.chat.Length, opensegment.Array, opensegment.Offset + count + sizeof(ushort));
		 success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), chatLen); // 이름길이(nameLen)을 버퍼에 쓴다.
		
		 count += sizeof(ushort); // count를 이름길이 필드 크기만큼 증가시킨다. (이름 길이를 저장하는 ushort 공간을 건너뛰기 위해)
		 count += chatLen; // count를 이름 데이터 크기만큼 증가시킨다. (실제 이름 데이터를 건너뛰기 위해)


        success &= BitConverter.TryWriteBytes(s, count); // 최종 패킷의 크기를 버퍼의 시작부분에 쓴다.

        if (success == false)
        {
            return null;
        }

        return SendBufferHelper.Close(count); // 직렬화 작업이 성공하면 버퍼를 닫고 직렬화된 데이터를 포함하는 어레이 세그먼트를 반환한다.

    }
}



class S_Chat : IPacket
{
    public int playerid;
	public string chat;
    
    public ushort Protocol { get { return (ushort)PacketID.S_Chat; } }
  
    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;

        //읽기 전용: ReadOnlySpan<T>는 데이터를 수정할 수 없도록 보장합니다. 이로 인해 데이터를 안전하게 읽을 수 있으며, 무결성이 유지됩니다.
        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array,segment.Offset,segment.Count);   
        count += sizeof(ushort); 
        count += sizeof(ushort); 

        this.playerid = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		 count += sizeof(int); 
		 ushort chatLen = BitConverter.ToUInt16(s.Slice(count, s.Length-count));
		count += sizeof(ushort);
		this.chat = Encoding.Unicode.GetString(s.Slice(count, chatLen));
		count += chatLen;


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


        success &= BitConverter.TryWriteBytes(s.Slice(count,s.Length - count), (ushort)PacketID.S_Chat); // packetid를 버퍼에 쓰고, Bitconverter.Trywritebytes 메서드는 데이터를 지정된 위치에 쓰고, 성공여부를 반환한다.
        count += sizeof(ushort); // count를 packetid 필드 크기만큼 증가시킨다.


        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerid);
		 count += sizeof(int);
		 ushort chatLen =(ushort) Encoding.Unicode.GetBytes(this.chat, 0, this.chat.Length, opensegment.Array, opensegment.Offset + count + sizeof(ushort));
		 success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), chatLen); // 이름길이(nameLen)을 버퍼에 쓴다.
		
		 count += sizeof(ushort); // count를 이름길이 필드 크기만큼 증가시킨다. (이름 길이를 저장하는 ushort 공간을 건너뛰기 위해)
		 count += chatLen; // count를 이름 데이터 크기만큼 증가시킨다. (실제 이름 데이터를 건너뛰기 위해)


        success &= BitConverter.TryWriteBytes(s, count); // 최종 패킷의 크기를 버퍼의 시작부분에 쓴다.

        if (success == false)
        {
            return null;
        }

        return SendBufferHelper.Close(count); // 직렬화 작업이 성공하면 버퍼를 닫고 직렬화된 데이터를 포함하는 어레이 세그먼트를 반환한다.

    }
}



