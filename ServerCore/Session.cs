using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    /// <summary>
    /// 세션 : 클라이언트와 서버 간의 대화나 상호작용이 지속되는 기간
    /// </summary>
    internal class Session
    {

        Socket _socket;
        private int _disconnected = 0;
        object _lock = new object();    
        Queue<byte[]> _send_queue = new Queue<byte[]>();
        bool _pending = false;
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs(); // _sendarg 재사용


        public void Start(Socket socket)
        {
            _socket = socket;

            SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);

            //recvArgs.UserToken = this; //이 세션으로 부터 온거다 라는 정보(this)
            
            recvArgs.SetBuffer(new byte[1024], 0, 1024);
            RegisterRecv(recvArgs);

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

        }

        public void Send(byte[] sendbuff)
        {
            lock (_lock)
            {
                _send_queue.Enqueue(sendbuff);
                if (_pending == false) // 실제로 연결된 유저가 없으면,
                {
                    RegisterSend();
                }

            }

        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        #region 네트워크 통신

        void RegisterSend()
        {

            _pending = true;
            byte[] buff = _send_queue.Dequeue();
            _sendArgs.SetBuffer(buff,0, buff.Length);

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
                        if(_send_queue.Count > 0) // 멀티스레드 환경으로, 내가 예약하는동안 누군가 예약을 했을때의 처리
                        {
                            RegisterSend();
                        }

                        else
                        {
                            _pending = false;
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
        void RegisterRecv(SocketAsyncEventArgs args)
        {
            bool pending = _socket.ReceiveAsync(args);
            if(pending == false)
            {
                OnRecvCompleted(null, args);
            }

        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if(args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                //TODO
                try
                {
                    string recvData = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
                    Console.WriteLine($"[From Client] : {recvData}");


                    RegisterRecv(args);

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
