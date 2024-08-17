using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace PacketGenerator
{
    /// <summary>
    /// 패킷 자동화를 위한 템플릿과 메서드를 제공합니다.
    /// </summary>
    class PacketFormat
    {
        // {0} 패킷 등록
        public static string managerFormat =


@"using ServerCore;


internal class PacketManager
{{ 
    #region Singleton
    static PacketManager _instance = new PacketManager();
    public static PacketManager Instance {{ get {{ return _instance; }} }}
    #endregion

     PacketManager()
    {{
        Register();
    }}


    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();
    Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();
    /// <summary>
    /// 자동화 메서드
    /// </summary>
    public void Register()
    {{

{0}

    }}



    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
    {{
        ushort count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Action<PacketSession, ArraySegment<byte>> action = null;
        if(_onRecv.TryGetValue(id, out action))
        {{
            action.Invoke(session, buffer);
        }}

    }}

    private void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T: IPacket, new()
    {{
        T packet = new T();
        packet.Read(buffer);


        Action<PacketSession, IPacket> action = null;
        if(_handler.TryGetValue(packet.Protocol, out action))
        {{
            action.Invoke(session, packet);
        }}
    }}

    }}";
        // {0} 패킷 이름
        public static string managerRegisterFormat =
@"       _onRecv.Add((ushort)PacketID.{0}, MakePacket<{0}>);
        _handler.Add((ushort)PacketID.{0}, PacketHandler.{0}Handler);";

        // {0} 패킷이름/번호목록
        // {1} 패킷 목록
        public static string fileFormat =
@"using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Collections.Specialized;


public enum PacketID
{{
    {0}
}}

interface IPacket
{{
	ushort Protocol {{ get; }}
	void Read(ArraySegment<byte> segment);
	ArraySegment<byte> Write();

}}

{1}

";
        // {0} 패킷 이름
        // {1} 패킷 번호
        public static string packetEnumFormat =
@"{0} = {1},";

        //{0} 패킷 이름
        //{1} 멤버 변수
        //{2} 멤버 변수 Read
        //{3} 멤버 변수 Write

        public static string packetFormat =

@"

class {0} : IPacket
{{
    {1}
    
    public ushort Protocol {{ get {{ return (ushort)PacketID.{0}; }} }}
  
    public void Read(ArraySegment<byte> segment)
    {{
        ushort count = 0;

        //읽기 전용: ReadOnlySpan<T>는 데이터를 수정할 수 없도록 보장합니다. 이로 인해 데이터를 안전하게 읽을 수 있으며, 무결성이 유지됩니다.
        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array,segment.Offset,segment.Count);   
        count += sizeof(ushort); 
        count += sizeof(ushort); 

       {2}


    }}

    public ArraySegment<byte> Write()
    {{
            
        ArraySegment<byte> opensegment = SendBufferHelper.Open(4096); // 4096 바이트 크기의 버퍼를 연다. 패킷 데이터를 저장하기 위해 사용된다.

        bool success = true; // 직렬화 작업의 성공여부를 나타내는 변수.
        ushort count = 0; // 직렬화 한 바이트 수 

        // 버퍼의 배열, 오프셋, 카운트를 기반으로 Span<byte>를 생성한다. 
        //Span<T>는 포인터와 유사한 기능을 제공하면서도 메모리 안전성을 보장합니다.
        //이는 배열, 문자열, 또는 기타 메모리 블록의 부분을 참조할 수 있습니다.
        Span<byte> s = new Span<byte>(opensegment.Array, opensegment.Offset, opensegment.Count);

        count += sizeof(ushort); // 패킷 크기 필드를 건너뛰기 위해 count를 ushort만큼 증가시킨다.


        success &= BitConverter.TryWriteBytes(s.Slice(count,s.Length - count), (ushort)PacketID.{0}); // packetid를 버퍼에 쓰고, Bitconverter.Trywritebytes 메서드는 데이터를 지정된 위치에 쓰고, 성공여부를 반환한다.
        count += sizeof(ushort); // count를 packetid 필드 크기만큼 증가시킨다.


        {3}


        success &= BitConverter.TryWriteBytes(s, count); // 최종 패킷의 크기를 버퍼의 시작부분에 쓴다.

        if (success == false)
        {{
            return null;
        }}

        return SendBufferHelper.Close(count); // 직렬화 작업이 성공하면 버퍼를 닫고 직렬화된 데이터를 포함하는 어레이 세그먼트를 반환한다.

    }}
}}

";


        // {0} 변수 형식
        // {1} 변수 이름
        public static string memberFormat =
@"public {0} {1};";

        //{0} 리스트 이름 [대문자]
        //{1} 리스트 이름 [소문자]
        //{2} 멤버 변수들
        //{3} 멤버 변수 Read
        //{4} 멤버 변수 Write

        public static string memberListFormat =
@" public class {0}
{{

    {2}



    public void Read(ReadOnlySpan<byte> s, ref ushort count)

    {{
        {3}

    }}

    public bool Write(Span<byte> s, ref ushort count) //여기서 Span은 전체 바이트 배열임. 두번째 인자는 실시간으로 우리가 몇번째 카운트를 작업하고 있는지.
   
    {{
        bool success = true;

        {4}

        return true;
    }}

   

}}


    public List<{0}> {1}s = new List<{0}>();";


        //{0} 변수 이름
        //{1} To~ 변수형식
        //{2} 변수 형식
        public static string readFormat =
@" this.{0} = BitConverter.{1}(s.Slice(count, s.Length - count));
 count += sizeof({2}); ";

        //{0} 변수이름
        //{1} 변수형식
        public static string readByteFormat =
@"this.{0} = ({1})segment.Array[segment.Offset + count] ;
count += sizeof({1});";


        //{0} 변수 이름
        public static string readStringFormat =
@" ushort {0}Len = BitConverter.ToUInt16(s.Slice(count, s.Length-count));
count += sizeof(ushort);
this.{0} = Encoding.Unicode.GetString(s.Slice(count, {0}Len));
count += {0}Len;";


        // {0} 리스트 이름 [대문자]
        // {1} 리스트 이름 {소문자}
        public static string readListFormat =

@" {1}s.Clear();
ushort {1}Len = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
count += sizeof(ushort);

for(int i = 0; i<{1}Len; i++)
{{
    {0} {1} = new {0}();
    {1}.Read(s, ref count);

    {1}s.Add({1});

}}";


        //{0} 변수 이름
        //{1} 변수 형식
        public static string writeFormat =
@"success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.{0});
 count += sizeof({1});";


        //{0} 변수이름
        //{1} 변수형식
        public static string writeByteFormat =
@"opensegment.Array[opensegment.Offset + count] = (byte)this.{0};
count += sizeof({1});";

        //{0} 변수 이름      
        public static string writeStringFormat =
@" ushort {0}Len =(ushort) Encoding.Unicode.GetBytes(this.{0}, 0, this.{0}.Length, opensegment.Array, opensegment.Offset + count + sizeof(ushort));
 success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), {0}Len); // 이름길이(nameLen)을 버퍼에 쓴다.

 count += sizeof(ushort); // count를 이름길이 필드 크기만큼 증가시킨다. (이름 길이를 저장하는 ushort 공간을 건너뛰기 위해)
 count += {0}Len; // count를 이름 데이터 크기만큼 증가시킨다. (실제 이름 데이터를 건너뛰기 위해)";

        // {0} 리스트 이름 [대문자]
        // {1} 리스트 이름 {소문자}
        public static string writeListFormat =
@"success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.{1}s.Count);
count += sizeof(ushort);

foreach({0} {1} in this.{1}s)
{{
    success &= {1}.Write(s, ref count);
}}";

    }
}
