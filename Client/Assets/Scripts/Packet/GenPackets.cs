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
    S_BroadcastEnterGame = 1,
	C_LeaveGame = 2,
	S_BroadcastLeaveGame = 3,
	S_PlayerList = 4,
	C_Move = 5,
	S_BroadcastMove = 6,
	
}

public interface IPacket
{
	ushort Protocol { get; }
	void Read(ArraySegment<byte> segment);
	ArraySegment<byte> Write();

}



public class S_BroadcastEnterGame : IPacket
{
    public int playerId;
	public float posX;
	public float posY;
	public float posZ;
    
    public ushort Protocol { get { return (ushort)PacketID.S_BroadcastEnterGame; } }
  
    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;      
        count += sizeof(ushort); 
        count += sizeof(ushort); 

       this.playerId = BitConverter.ToInt32(segment.Array, segment.Offset + count);
		 count += sizeof(int); 
		this.posX = BitConverter.ToSingle(segment.Array, segment.Offset + count);
		 count += sizeof(float); 
		this.posY = BitConverter.ToSingle(segment.Array, segment.Offset + count);
		 count += sizeof(float); 
		this.posZ = BitConverter.ToSingle(segment.Array, segment.Offset + count);
		 count += sizeof(float); 


    }

    public ArraySegment<byte> Write()
    {
            
        ArraySegment<byte> opensegment = SendBufferHelper.Open(4096); // 4096 바이트 크기의 버퍼를 연다. 패킷 데이터를 저장하기 위해 사용된다.
    
        ushort count = 0; // 직렬화 한 바이트 수 

       
        count += sizeof(ushort); // 패킷 크기 필드를 건너뛰기 위해 count를 ushort만큼 증가시킨다.
        Array.Copy(BitConverter.GetBytes((ushort)PacketID.S_BroadcastEnterGame), 0, opensegment.Array, opensegment.Offset + count, sizeof(ushort));              
        count += sizeof(ushort); // count를 packetid 필드 크기만큼 증가시킨다.

        Array.Copy(BitConverter.GetBytes(this.playerId), 0, opensegment.Array, opensegment.Offset + count, sizeof(int));
		 count += sizeof(int);
		Array.Copy(BitConverter.GetBytes(this.posX), 0, opensegment.Array, opensegment.Offset + count, sizeof(float));
		 count += sizeof(float);
		Array.Copy(BitConverter.GetBytes(this.posY), 0, opensegment.Array, opensegment.Offset + count, sizeof(float));
		 count += sizeof(float);
		Array.Copy(BitConverter.GetBytes(this.posZ), 0, opensegment.Array, opensegment.Offset + count, sizeof(float));
		 count += sizeof(float);
        
        Array.Copy(BitConverter.GetBytes(count), 0, opensegment.Array, opensegment.Offset, sizeof(ushort));       

        return SendBufferHelper.Close(count); // 직렬화 작업이 성공하면 버퍼를 닫고 직렬화된 데이터를 포함하는 어레이 세그먼트를 반환한다.

    }
}



public class C_LeaveGame : IPacket
{
    
    
    public ushort Protocol { get { return (ushort)PacketID.C_LeaveGame; } }
  
    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;      
        count += sizeof(ushort); 
        count += sizeof(ushort); 

       


    }

    public ArraySegment<byte> Write()
    {
            
        ArraySegment<byte> opensegment = SendBufferHelper.Open(4096); // 4096 바이트 크기의 버퍼를 연다. 패킷 데이터를 저장하기 위해 사용된다.
    
        ushort count = 0; // 직렬화 한 바이트 수 

       
        count += sizeof(ushort); // 패킷 크기 필드를 건너뛰기 위해 count를 ushort만큼 증가시킨다.
        Array.Copy(BitConverter.GetBytes((ushort)PacketID.C_LeaveGame), 0, opensegment.Array, opensegment.Offset + count, sizeof(ushort));              
        count += sizeof(ushort); // count를 packetid 필드 크기만큼 증가시킨다.

        
        
        Array.Copy(BitConverter.GetBytes(count), 0, opensegment.Array, opensegment.Offset, sizeof(ushort));       

        return SendBufferHelper.Close(count); // 직렬화 작업이 성공하면 버퍼를 닫고 직렬화된 데이터를 포함하는 어레이 세그먼트를 반환한다.

    }
}



public class S_BroadcastLeaveGame : IPacket
{
    public int playerId;
    
    public ushort Protocol { get { return (ushort)PacketID.S_BroadcastLeaveGame; } }
  
    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;      
        count += sizeof(ushort); 
        count += sizeof(ushort); 

       this.playerId = BitConverter.ToInt32(segment.Array, segment.Offset + count);
		 count += sizeof(int); 


    }

    public ArraySegment<byte> Write()
    {
            
        ArraySegment<byte> opensegment = SendBufferHelper.Open(4096); // 4096 바이트 크기의 버퍼를 연다. 패킷 데이터를 저장하기 위해 사용된다.
    
        ushort count = 0; // 직렬화 한 바이트 수 

       
        count += sizeof(ushort); // 패킷 크기 필드를 건너뛰기 위해 count를 ushort만큼 증가시킨다.
        Array.Copy(BitConverter.GetBytes((ushort)PacketID.S_BroadcastLeaveGame), 0, opensegment.Array, opensegment.Offset + count, sizeof(ushort));              
        count += sizeof(ushort); // count를 packetid 필드 크기만큼 증가시킨다.

        Array.Copy(BitConverter.GetBytes(this.playerId), 0, opensegment.Array, opensegment.Offset + count, sizeof(int));
		 count += sizeof(int);
        
        Array.Copy(BitConverter.GetBytes(count), 0, opensegment.Array, opensegment.Offset, sizeof(ushort));       

        return SendBufferHelper.Close(count); // 직렬화 작업이 성공하면 버퍼를 닫고 직렬화된 데이터를 포함하는 어레이 세그먼트를 반환한다.

    }
}



public class S_PlayerList : IPacket
{
     public class Player
	{
	
	    public bool isSelf;
		public int playerId;
		public float posX;
		public float posY;
		public float posZ;
	
	
	
	    public void Read(ArraySegment<byte> segment, ref ushort count)
	
	    {
	        this.isSelf = BitConverter.ToBoolean(segment.Array, segment.Offset + count);
			 count += sizeof(bool); 
			this.playerId = BitConverter.ToInt32(segment.Array, segment.Offset + count);
			 count += sizeof(int); 
			this.posX = BitConverter.ToSingle(segment.Array, segment.Offset + count);
			 count += sizeof(float); 
			this.posY = BitConverter.ToSingle(segment.Array, segment.Offset + count);
			 count += sizeof(float); 
			this.posZ = BitConverter.ToSingle(segment.Array, segment.Offset + count);
			 count += sizeof(float); 
	
	    }
	
	    public bool Write(ArraySegment<byte> segment, ref ushort count) //여기서 Span은 전체 바이트 배열임. 두번째 인자는 실시간으로 우리가 몇번째 카운트를 작업하고 있는지.
	   
	    {
	        bool success = true;
	
	        Array.Copy(BitConverter.GetBytes(this.isSelf), 0, opensegment.Array, opensegment.Offset + count, sizeof(bool));
			 count += sizeof(bool);
			Array.Copy(BitConverter.GetBytes(this.playerId), 0, opensegment.Array, opensegment.Offset + count, sizeof(int));
			 count += sizeof(int);
			Array.Copy(BitConverter.GetBytes(this.posX), 0, opensegment.Array, opensegment.Offset + count, sizeof(float));
			 count += sizeof(float);
			Array.Copy(BitConverter.GetBytes(this.posY), 0, opensegment.Array, opensegment.Offset + count, sizeof(float));
			 count += sizeof(float);
			Array.Copy(BitConverter.GetBytes(this.posZ), 0, opensegment.Array, opensegment.Offset + count, sizeof(float));
			 count += sizeof(float);
	
	        return success;
	    }
	
	   
	
	}
	
	
	    public List<Player> players = new List<Player>();
    
    public ushort Protocol { get { return (ushort)PacketID.S_PlayerList; } }
  
    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;      
        count += sizeof(ushort); 
        count += sizeof(ushort); 

        players.Clear();
		ushort playerLen = BitConverter.ToUInt16(segment.Array, segment.Offset + count);
		count += sizeof(ushort);
		
		for(int i = 0; i<playerLen; i++)
		{
		    Player player = new Player();
		    player.Read(s, ref count);
		
		    players.Add(player);
		
		}


    }

    public ArraySegment<byte> Write()
    {
            
        ArraySegment<byte> opensegment = SendBufferHelper.Open(4096); // 4096 바이트 크기의 버퍼를 연다. 패킷 데이터를 저장하기 위해 사용된다.
    
        ushort count = 0; // 직렬화 한 바이트 수 

       
        count += sizeof(ushort); // 패킷 크기 필드를 건너뛰기 위해 count를 ushort만큼 증가시킨다.
        Array.Copy(BitConverter.GetBytes((ushort)PacketID.S_PlayerList), 0, opensegment.Array, opensegment.Offset + count, sizeof(ushort));              
        count += sizeof(ushort); // count를 packetid 필드 크기만큼 증가시킨다.

        Array.Copy(BitConverter.GetBytes((ushort)this.players.Count), 0, opensegment.Array, opensegment.Offset + count, sizeof(ushort));
		count += sizeof(ushort);
		
		foreach(Player player in this.players)
		{
		    player.Write(opensegment, ref count);
		}
        
        Array.Copy(BitConverter.GetBytes(count), 0, opensegment.Array, opensegment.Offset, sizeof(ushort));       

        return SendBufferHelper.Close(count); // 직렬화 작업이 성공하면 버퍼를 닫고 직렬화된 데이터를 포함하는 어레이 세그먼트를 반환한다.

    }
}



public class C_Move : IPacket
{
    public float posX;
	public float posY;
	public float posZ;
    
    public ushort Protocol { get { return (ushort)PacketID.C_Move; } }
  
    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;      
        count += sizeof(ushort); 
        count += sizeof(ushort); 

       this.posX = BitConverter.ToSingle(segment.Array, segment.Offset + count);
		 count += sizeof(float); 
		this.posY = BitConverter.ToSingle(segment.Array, segment.Offset + count);
		 count += sizeof(float); 
		this.posZ = BitConverter.ToSingle(segment.Array, segment.Offset + count);
		 count += sizeof(float); 


    }

    public ArraySegment<byte> Write()
    {
            
        ArraySegment<byte> opensegment = SendBufferHelper.Open(4096); // 4096 바이트 크기의 버퍼를 연다. 패킷 데이터를 저장하기 위해 사용된다.
    
        ushort count = 0; // 직렬화 한 바이트 수 

       
        count += sizeof(ushort); // 패킷 크기 필드를 건너뛰기 위해 count를 ushort만큼 증가시킨다.
        Array.Copy(BitConverter.GetBytes((ushort)PacketID.C_Move), 0, opensegment.Array, opensegment.Offset + count, sizeof(ushort));              
        count += sizeof(ushort); // count를 packetid 필드 크기만큼 증가시킨다.

        Array.Copy(BitConverter.GetBytes(this.posX), 0, opensegment.Array, opensegment.Offset + count, sizeof(float));
		 count += sizeof(float);
		Array.Copy(BitConverter.GetBytes(this.posY), 0, opensegment.Array, opensegment.Offset + count, sizeof(float));
		 count += sizeof(float);
		Array.Copy(BitConverter.GetBytes(this.posZ), 0, opensegment.Array, opensegment.Offset + count, sizeof(float));
		 count += sizeof(float);
        
        Array.Copy(BitConverter.GetBytes(count), 0, opensegment.Array, opensegment.Offset, sizeof(ushort));       

        return SendBufferHelper.Close(count); // 직렬화 작업이 성공하면 버퍼를 닫고 직렬화된 데이터를 포함하는 어레이 세그먼트를 반환한다.

    }
}



public class S_BroadcastMove : IPacket
{
    public int playerId;
	public float posX;
	public float posY;
	public float posZ;
    
    public ushort Protocol { get { return (ushort)PacketID.S_BroadcastMove; } }
  
    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;      
        count += sizeof(ushort); 
        count += sizeof(ushort); 

       this.playerId = BitConverter.ToInt32(segment.Array, segment.Offset + count);
		 count += sizeof(int); 
		this.posX = BitConverter.ToSingle(segment.Array, segment.Offset + count);
		 count += sizeof(float); 
		this.posY = BitConverter.ToSingle(segment.Array, segment.Offset + count);
		 count += sizeof(float); 
		this.posZ = BitConverter.ToSingle(segment.Array, segment.Offset + count);
		 count += sizeof(float); 


    }

    public ArraySegment<byte> Write()
    {
            
        ArraySegment<byte> opensegment = SendBufferHelper.Open(4096); // 4096 바이트 크기의 버퍼를 연다. 패킷 데이터를 저장하기 위해 사용된다.
    
        ushort count = 0; // 직렬화 한 바이트 수 

       
        count += sizeof(ushort); // 패킷 크기 필드를 건너뛰기 위해 count를 ushort만큼 증가시킨다.
        Array.Copy(BitConverter.GetBytes((ushort)PacketID.S_BroadcastMove), 0, opensegment.Array, opensegment.Offset + count, sizeof(ushort));              
        count += sizeof(ushort); // count를 packetid 필드 크기만큼 증가시킨다.

        Array.Copy(BitConverter.GetBytes(this.playerId), 0, opensegment.Array, opensegment.Offset + count, sizeof(int));
		 count += sizeof(int);
		Array.Copy(BitConverter.GetBytes(this.posX), 0, opensegment.Array, opensegment.Offset + count, sizeof(float));
		 count += sizeof(float);
		Array.Copy(BitConverter.GetBytes(this.posY), 0, opensegment.Array, opensegment.Offset + count, sizeof(float));
		 count += sizeof(float);
		Array.Copy(BitConverter.GetBytes(this.posZ), 0, opensegment.Array, opensegment.Offset + count, sizeof(float));
		 count += sizeof(float);
        
        Array.Copy(BitConverter.GetBytes(count), 0, opensegment.Array, opensegment.Offset, sizeof(ushort));       

        return SendBufferHelper.Close(count); // 직렬화 작업이 성공하면 버퍼를 닫고 직렬화된 데이터를 포함하는 어레이 세그먼트를 반환한다.

    }
}



