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
        count += sizeof(ushort); 
        count += sizeof(ushort); 

        ushort chatLen = BitConverter.ToUInt16(segment.Array, segment.Offset + count);
		count += sizeof(ushort);
		this.chat = Encoding.Unicode.GetString(segment.Array, segment.Offset + count, chatLen);
		count += chatLen;


    }

    public ArraySegment<byte> Write()
    {
            
        ArraySegment<byte> opensegment = SendBufferHelper.Open(4096); // 4096 바이트 크기의 버퍼를 연다. 패킷 데이터를 저장하기 위해 사용된다.
    
        ushort count = 0; // 직렬화 한 바이트 수 

       
        count += sizeof(ushort); // 패킷 크기 필드를 건너뛰기 위해 count를 ushort만큼 증가시킨다.
        Array.Copy(BitConverter.GetBytes((ushort)PacketID.C_Chat), 0, opensegment.Array, opensegment.Offset + count, sizeof(ushort));              
        count += sizeof(ushort); // count를 packetid 필드 크기만큼 증가시킨다.

         ushort chatLen =(ushort) Encoding.Unicode.GetBytes(this.chat, 0, this.chat.Length, opensegment.Array, opensegment.Offset + count + sizeof(ushort));
		Array.Copy(BitConverter.GetBytes(chatLen), 0, opensegment.Array, opensegment.Offset + count, sizeof(ushort));
		count += sizeof(ushort); // count를 이름길이 필드 크기만큼 증가시킨다. (이름 길이를 저장하는 ushort 공간을 건너뛰기 위해)
		count += chatLen; // count를 이름 데이터 크기만큼 증가시킨다. (실제 이름 데이터를 건너뛰기 위해)
        
        Array.Copy(BitConverter.GetBytes(count), 0, opensegment.Array, opensegment.Offset, sizeof(ushort));       

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
        count += sizeof(ushort); 
        count += sizeof(ushort); 

       this.playerid = BitConverter.ToInt32(segment.Array, segment.Offset + count);
		 count += sizeof(int); 
		 ushort chatLen = BitConverter.ToUInt16(segment.Array, segment.Offset + count);
		count += sizeof(ushort);
		this.chat = Encoding.Unicode.GetString(segment.Array, segment.Offset + count, chatLen);
		count += chatLen;


    }

    public ArraySegment<byte> Write()
    {
            
        ArraySegment<byte> opensegment = SendBufferHelper.Open(4096); // 4096 바이트 크기의 버퍼를 연다. 패킷 데이터를 저장하기 위해 사용된다.
    
        ushort count = 0; // 직렬화 한 바이트 수 

       
        count += sizeof(ushort); // 패킷 크기 필드를 건너뛰기 위해 count를 ushort만큼 증가시킨다.
        Array.Copy(BitConverter.GetBytes((ushort)PacketID.S_Chat), 0, opensegment.Array, opensegment.Offset + count, sizeof(ushort));              
        count += sizeof(ushort); // count를 packetid 필드 크기만큼 증가시킨다.

        Array.Copy(BitConverter.GetBytes(this.playerid), 0, opensegment.Array, opensegment.Offset + count, sizeof(int));
		 count += sizeof(int);
		 ushort chatLen =(ushort) Encoding.Unicode.GetBytes(this.chat, 0, this.chat.Length, opensegment.Array, opensegment.Offset + count + sizeof(ushort));
		Array.Copy(BitConverter.GetBytes(chatLen), 0, opensegment.Array, opensegment.Offset + count, sizeof(ushort));
		count += sizeof(ushort); // count를 이름길이 필드 크기만큼 증가시킨다. (이름 길이를 저장하는 ushort 공간을 건너뛰기 위해)
		count += chatLen; // count를 이름 데이터 크기만큼 증가시킨다. (실제 이름 데이터를 건너뛰기 위해)
        
        Array.Copy(BitConverter.GetBytes(count), 0, opensegment.Array, opensegment.Offset, sizeof(ushort));       

        return SendBufferHelper.Close(count); // 직렬화 작업이 성공하면 버퍼를 닫고 직렬화된 데이터를 포함하는 어레이 세그먼트를 반환한다.

    }
}



