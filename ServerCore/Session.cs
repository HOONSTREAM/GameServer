using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    /// <summary>
    /// 세션 : 클라이언트와 서버 간의 대화나 상호작용이 지속되는 기간
    /// </summary>
    public abstract class Session
    {

        Socket _socket; 
        private int _disconnected = 0;
        object _lock = new object();    
        Queue<byte[]> _send_queue = new Queue<byte[]>();      

        List<ArraySegment<byte>> _pendinglist = new List<ArraySegment<byte>>(); //이전버전에서 큐 대신 사용되는 것이며, 대기목록 리스트임
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs(); // _sendarg 재사용
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint endpoint);
        public abstract void OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endpoint);

        public void Start(Socket socket)
        {
            _socket = socket;


            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);

            //recvArgs.UserToken = this; //이 세션으로 부터 온거다 라는 정보(this)

            _recvArgs.SetBuffer(new byte[1024], 0, 1024);
            RegisterRecv();

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

        }

        public void Send(byte[] sendbuff)
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
        }

        #region 네트워크 통신

        void RegisterSend()
        {

            while (_send_queue.Count > 0)
            {
                byte[] buff = _send_queue.Dequeue();
                _pendinglist.Add(new ArraySegment<byte>(buff,0,buff.Length));   // 어떤 배열의 일부를 나타내는 구조체(스택) , 패킷 모아보내기
            }
            _sendArgs.BufferList = _pendinglist;

            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
            {
                OnSendCompleted(null, _sendArgs);
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
            bool pending = _socket.ReceiveAsync(_recvArgs);
            if(pending == false)
            {
                OnRecvCompleted(null, _recvArgs);
            }

        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if(args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                
                try
                {
                    OnRecv(new ArraySegment<byte>(args.Buffer, args.Offset, args.BytesTransferred));
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
