using Server;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    // internal 접근 수준은 같은 어셈블리 내에서만 접근할 수 있도록 제한합니다.
    // 즉, 동일한 프로젝트 내에서만 해당 멤버를 사용할 수 있으며, 다른 프로젝트에서는 접근할 수 없습니다.
    class PacketHandler

    {
        public static void C_ChatHandler(PacketSession session, IPacket packet)
        {
            C_Chat chatPacket = packet as C_Chat;
            ClientSession clientsession = session as ClientSession;

            if(clientsession.Room == null) { return; }

        clientsession.Room.Broadcast(clientsession, chatPacket.chat);

        }

       


}

