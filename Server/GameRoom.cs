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

            //Console.WriteLine($"Flushed {_pendingList.Count} items");
            _pendingList.Clear();
        }
        /// <summary>
        /// 어떤 한 공간에서 유저들이 모여있고, 한 유저가 하는 행동을 전체로 뿌려주는 작업 (브로드캐스팅)
        /// </summary>
        /// <param name="session"></param>
        /// <param name="chat"></param>
        public void Broadcast(ArraySegment<byte> segment)
        {           
            _pendingList.Add(segment);                  
        }

        public void Enter(ClientSession session)
        {   
            // 플레이어 추가
            _sessions.Add(session);
            session.Room = this;
            
            // 신입생한테 모든 플레이어 목록 전송
            S_PlayerList players = new S_PlayerList();
            foreach (ClientSession s in _sessions)
            {
                players.players.Add(new S_PlayerList.Player()
                {
                    isSelf =(s == session),
                    playerId = s.SessionId,
                    posX = s.PosX,
                    posY = s.PosY,
                    posZ = s.PosZ,

                });
            }

            session.Send(players.Write());

            // 신입생 입장을 모두에게 알린다.
            S_BroadcastEnterGame enter = new S_BroadcastEnterGame();
            enter.playerId = session.SessionId;
            enter.posX = 0;
            enter.posY = 0;
            enter.posZ = 0;
            Broadcast(enter.Write());
        }
        public void Leave(ClientSession session)
        {           
            //플레이어 제거
           _sessions.Remove(session);  
            //플레이어 제거를 모두에게 알린다.
            S_BroadcastLeaveGame leave = new S_BroadcastLeaveGame();
            leave.playerId = session.SessionId;
            Broadcast(leave.Write());
        }

        public void Move(ClientSession session, C_Move packet)
        {
            // 좌표를 바꾼다

            session.PosX = packet.posX;
            session.PosY = packet.posY;               
            session.PosZ = packet.posZ;

            // 모두에게 알린다.

            S_BroadcastMove move = new S_BroadcastMove();
            move.playerId = session.SessionId;
            move.posX = session.PosX;
            move.posY = session.PosY;
            move.posZ = session.PosZ;
            Broadcast(move.Write());
        }

       
    }
}
