using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class GameRoom
    {
        /// <summary>
        /// 리스트나 딕셔너리와 같이 대부분의 자료구조들은 멀티스레드 환경을 보장하지 않는다.
        /// </summary>
        List<ClientSession> _sessions = new List<ClientSession>();
        object _lock = new object();

        public void Broadcast(ClientSession session, string chat)
        {
            S_Chat packet = new S_Chat();
            packet.playerid = session.SessionId;
            packet.chat = chat;
            ArraySegment<byte> segment = packet.Write();

            lock (_lock)
            {
                foreach(ClientSession s in _sessions)
                {
                    s.Send(segment);
                }
            }
        }
        public void Enter(ClientSession session)
        {
            lock(_lock)
            {
                _sessions.Add(session);
                session.Room = this;
            }
          
        }

        public void Leave(ClientSession session)
        {
            lock(_lock)
            {
                _sessions.Remove(session);
            }
           
        }




    }
}
