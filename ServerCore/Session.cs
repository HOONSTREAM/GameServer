using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{

    public abstract class PacketSession : Session
    {
        public static readonly short HeaderSize = 2;
       
        public sealed override int OnRecv(ArraySegment<byte> buffer) /// 다른클래스가 PacketSession 클래스를 상속받아도, OnRecv를 오버라이딩 할 수 없다. (Sealed 키워드)
        {
            int processLen = 0;
            int packetcount = 0;

            while (true)
            {
                // 최소한 헤더는 파싱할 수 있는지 확인한다.

                if(buffer.Count < HeaderSize)
                {
                    break;
                }

                //패킷이 완전히 도착했는지 확인한다.
               ushort dataSize =  BitConverter.ToUInt16(buffer.Array, buffer.Offset);

                if(buffer.Count < dataSize)
                {
                    break;
                }

                //여기까지 왔으면 패킷 조립 가능
                OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));
                packetcount++;
                processLen += dataSize;

                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
            }

            if(packetcount > 1)
            {
                Console.WriteLine($"패킷 모아보낸 숫자 : {packetcount}");
            }

            return processLen;
        }

        public abstract void OnRecvPacket(ArraySegment<byte> buffer);
    }
    /// <summary>
    /// 세션 : 클라이언트와 서버 간의 대화나 상호작용이 지속되는 기간
    /// </summary>
    public abstract class Session
    {

        Socket _socket; 
        private int _disconnected = 0;

        RecvBuffer _recvbuffer = new RecvBuffer(65535); //유저가 각기 보내는 데이터가 다를것이기 때문에 내부에 복사하여 들고 있는것이 맞음.

        object _lock = new object();    
        
        Queue<ArraySegment<byte>> _send_queue = new Queue<ArraySegment<byte>>();      
        List<ArraySegment<byte>> _pendinglist = new List<ArraySegment<byte>>(); //이전버전에서 큐 대신 사용되는 것이며, 대기목록 리스트임
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs(); // _sendarg 재사용
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();


        public abstract void OnConnected(EndPoint endpoint);
        public abstract int OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endpoint);

        private void Clear()
        {
            lock (_lock)
            {
                _send_queue.Clear();
                _pendinglist.Clear();
            }
        }
        public void Start(Socket socket)
        {
            _socket = socket;


            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            //recvArgs.UserToken = this; //이 세션으로 부터 온거다 라는 정보(this)
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
        }

        /// <summary>
        /// send 메서드 오버로딩
        /// </summary>
        /// <param name="sendbuffList"></param>
        public void Send(List<ArraySegment<byte>> sendbuffList)
        {
            if(sendbuffList.Count == 0) { return; }
            
            lock (_lock)
            {
                foreach (ArraySegment<byte> sendbuff in sendbuffList)
                {
                    _send_queue.Enqueue(sendbuff);
                }
               
                if (_pendinglist.Count == 0) // 실제로 연결된 유저가 없으면,
                {
                    RegisterSend();
                }

            }

        }

        public void Send(ArraySegment<byte> sendbuff)
        {
           
            lock (_lock)
            {
                _send_queue.Enqueue(sendbuff);

                if (_pendinglist.Count == 0) // 실제로 연결된 유저가 없으면,
                {
                    RegisterSend();
                }

            }

        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;

            OnDisconnected(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            Clear();
        }

        #region 네트워크 통신

        void RegisterSend()
        {
            if(_disconnected == 1)
            {
                return;
            }

            while (_send_queue.Count > 0)
            {
                ArraySegment<byte> buff = _send_queue.Dequeue();
                _pendinglist.Add(buff);   // 어떤 배열의 일부를 나타내는 구조체(스택) , 패킷 모아보내기
            }
            _sendArgs.BufferList = _pendinglist;

            try
            {
                bool pending = _socket.SendAsync(_sendArgs);
                if (pending == false)
                {
                    OnSendCompleted(null, _sendArgs);
                }
            }

            catch(Exception ex)
            {
                Console.WriteLine($"RegisterSend Failed {ex}");
            }
           

        }

        void OnSendCompleted (object sender, SocketAsyncEventArgs args)
        {
            lock ( _lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success) //BytesTransferred는 적어도 1바이트 이상이 성공적으로 전송되었는지 검사하는 조건
                {

                    try
                    {
                        _sendArgs.BufferList = null;
                        _pendinglist.Clear();

                        OnSend(_sendArgs.BytesTransferred);
                       

                        if (_send_queue.Count > 0) // 멀티스레드 환경으로, 내가 예약하는동안 누군가 예약을 했을때의 처리
                        {
                            RegisterSend();
                        }

                      
                    }

                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Failed {e}");
                    }
                }

                else
                {
                    Disconnect();
                }
            }
        
        }
        void RegisterRecv()
        {
            if(_disconnected == 1)
            {
                return;

            }
            _recvbuffer.Clean(); 

            ArraySegment<byte> segment = _recvbuffer.RecvSegment;
            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);


            try
            {
                bool pending = _socket.ReceiveAsync(_recvArgs);

                if (pending == false)
                {
                    OnRecvCompleted(null, _recvArgs);
                }
            }

            catch(Exception ex)
            {
                Console.WriteLine($" RegisterRecv Failed : {ex}");
            }
           

        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if(args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {

                try
                {
                    if (_recvbuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }

                    // 컨텐츠 쪽으로 데이터를 넘겨주고 얼마나 처리했는지 받는다.

                    int process_length = OnRecv(_recvbuffer.DataSegment);
                    if (process_length < 0 || _recvbuffer.DataSize < process_length)
                    {
                        Disconnect();
                        return;
                    }
                    //Read 커서 이동
                    if(_recvbuffer.OnRead(process_length) == false)
                    {
                        Disconnect();
                        return;
                    }

                    RegisterRecv();

                }

                catch(Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted Failed {e}");
                }


            }

            else
            {
                Disconnect();
            }
        }

        #endregion

    }
}
