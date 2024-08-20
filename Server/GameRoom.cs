using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class GameRoom : IJobQueue
    {
        /// <summary>
        /// 리스트나 딕셔너리와 같이 대부분의 자료구조들은 멀티스레드 환경을 보장하지 않는다.
        /// </summary>
        List<ClientSession> _sessions = new List<ClientSession>();       
        JobQueue _jobQueue = new JobQueue();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();


        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }


        public void Flush()
        {             
            foreach (ClientSession s in _sessions)
            {
                s.Send(_pendingList);
            }

            Console.WriteLine($"Flushed {_pendingList.Count} items");
            _pendingList.Clear();
        }
        /// <summary>
        /// 어떤 한 공간에서 유저들이 모여있고, 한 유저가 하는 행동을 전체로 뿌려주는 작업 (브로드캐스팅)
        /// </summary>
        /// <param name="session"></param>
        /// <param name="chat"></param>
        public void Broadcast(ClientSession session, string chat)
        {
            S_Chat packet = new S_Chat();
            packet.playerid = session.SessionId;
            packet.chat =  $"{chat} I am {packet.playerid}";
            ArraySegment<byte> segment = packet.Write();

            _pendingList.Add(segment);
                    
        }

        public void Enter(ClientSession session)
        {          
            _sessions.Add(session);
            session.Room = this;                     
        }
        public void Leave(ClientSession session)
        {           
           _sessions.Remove(session);                     
        }

       
    }
}
