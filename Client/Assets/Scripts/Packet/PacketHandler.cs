using DummyClient;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

     class PacketHandler
    {
        public static void S_ChatHandler(PacketSession session, IPacket packet)
        {
            S_Chat chatPacket = packet as S_Chat;
            ServerSession serversession = session as ServerSession;

            Debug.Log(chatPacket.chat);

            //if(chatPacket.playerid == 1)
            {
               // Console.WriteLine(chatPacket.chat);
            }

        }

    }

