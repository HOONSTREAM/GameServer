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

        
    }

    void Update()
    {
        List<IPacket> list = PacketQueue.Instance.PopAll();

        foreach (IPacket packet in list)
        {
            PacketManager.Instance.HandlePacket(_session, packet);
        }
        
              
    }

    public void Send(ArraySegment<byte> data)
    {
        _session.Send(data);
    }

}
