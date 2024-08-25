using DummyClient;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class NetWorkManager : MonoBehaviour
{   
    ServerSession _session = new ServerSession();
    void Start()
    {
        string host = System.Net.Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

        Connector connecter = new Connector();

        connecter.Connect(endPoint, () => { return _session; }, 1);

        StartCoroutine(CoSendPacket());
    }

    void Update()
    {
       IPacket packet =  PacketQueue.Instance.Pop();

        if(packet != null)
        {
            PacketManager.Instance.HandlePacket(_session, packet);
        }
        
    }

    IEnumerator CoSendPacket()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f);
            C_Chat chatpacket = new C_Chat();
            chatpacket.chat = "Hello Unity !";
            ArraySegment<byte> segment = chatpacket.Write();

            _session.Send(segment);
        }
    }
}
