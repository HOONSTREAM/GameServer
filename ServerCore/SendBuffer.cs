using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{

    public class SendBufferHelper
    {
        //레이스컨디션 방지
        public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(() => { return null; });

        public static int ChunkSize { get; set; } = 4096;

        public static ArraySegment<byte> Open (int reserveSize)
        {
            if(CurrentBuffer.Value == null) { CurrentBuffer.Value = new SendBuffer(ChunkSize); }

            if(CurrentBuffer.Value.FressSize < reserveSize)
            {
                CurrentBuffer.Value = new SendBuffer(ChunkSize);
            }

            return CurrentBuffer.Value.Open (reserveSize);
        }

        public static ArraySegment<byte> Close (int usedSize)
        {
            return CurrentBuffer.Value.Close(usedSize);
        }

    }
    public class SendBuffer
    {
        byte[] _buffer;
        int _usedSize = 0;

        public int FressSize { get { return _buffer.Length - _usedSize; } }

        public SendBuffer(int ChunkSize)
        {
            _buffer = new byte[ChunkSize];
        }

        public ArraySegment<byte> Open(int reserveSize) // 버퍼를 오픈하면서 얼마만큼의 사이즈를 사용할건지 
        {
            if(reserveSize > FressSize)
            {
                return null;
            }

            return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
        }

        public ArraySegment<byte> Close(int usedSize)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
            _usedSize += usedSize;

            return segment;
        }


         

    }
}
