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
        public static void C_PlayerInfoReqHandler(PacketSession session, IPacket packet)
        {
        C_PlayerInfoReq p = packet as C_PlayerInfoReq;

            Console.WriteLine($"PlayerInfoReq : {p.PlayerId}");
            Console.WriteLine($"Playername : {p.name}");

            foreach (C_PlayerInfoReq.Skill skill in p.skills)
            {
                Console.WriteLine($"Skill : {skill.id} , {skill.level}, {skill.duration}");
            }
        }

       


}

