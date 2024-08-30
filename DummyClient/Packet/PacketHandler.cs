using DummyClient;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

     class PacketHandler
    {
        public static void S_BroadcastEnterGameHandler(PacketSession session, IPacket packet)
        {
            S_BroadcastEnterGame pkt = packet as S_BroadcastEnterGame;
            ServerSession serversession = session as ServerSession;         
        }

    public static void S_BroadcastLeaveGameHandler(PacketSession session, IPacket packet)
    {
        S_BroadcastLeaveGame pkt = packet as S_BroadcastLeaveGame;
        ServerSession serversession = session as ServerSession;
    }

    public static void S_PlayerListHandler(PacketSession session, IPacket packet)
    {
        S_PlayerList pkt = packet as S_PlayerList;
        ServerSession serversession = session as ServerSession;
    }

    public static void S_BroadcastMoveHandler(PacketSession session, IPacket packet)
    {
        S_BroadcastMove pkt = packet as S_BroadcastMove;
        ServerSession serversession = session as ServerSession;
    }



}

