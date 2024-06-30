using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class RecvBuffer
    {
        ArraySegment<byte> _buffer;
        int _readPos;
        int _writePos;


        public RecvBuffer(int buffersize)
        {
            _buffer = new ArraySegment<byte>(new byte[buffersize], 0 ,buffersize);
        }

        public int DataSize { get { return _writePos - _readPos; } }
        public int FreeSize { get { return _buffer.Count - _writePos; } }   

        public ArraySegment<byte> DataSegment // 현재까지 받은 데이터의 범위가 어디서부터 어디까지인가?
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); } // 첫 시작위치 : _buffer.Array, 시작할 수 있는 위치 :Offset , 크기 : DataSize
        }

        public ArraySegment<byte> RecvSegment // 다음 Recv를 할 때 유효범위가 어디부터 어디까지인지?
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset+_writePos, FreeSize); }
        }

        public void Clean()
        {
            int datasize = DataSize;
            if(datasize == 0) // ReadPos과 DataPos가 겹치는 상황 즉, 버퍼에 아무것도 쓰여져있지 않은 상황

            {
                //남은 데이터가 없으면 복사하지 않고 커서 위치만 옮긴다.
                _readPos = _writePos = 0;
            }

            else // 버퍼에 남은 데이터가 있다.
            {
                Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, datasize);
                _readPos = 0;
                _writePos = datasize;
            }
        }

        /// <summary>
        /// Read Cursor를 이동합니다.
        /// </summary>
        /// <param name="numOfbytes"></param>
        /// <returns></returns>
        public bool OnRead(int numOfbytes) 
        {
            if(numOfbytes > DataSize) { return false; }

            _readPos+= numOfbytes;

            return true;
        }

        /// <summary>
        /// WriteCursor를 이동합니다.
        /// </summary>
        /// <param name="numOfbytes"></param>
        /// <returns></returns>
        public bool OnWrite(int numOfbytes)
        {
            if(numOfbytes > FreeSize) { return false; }
             
            _writePos+= numOfbytes;

            return true;
        }


    }
}
