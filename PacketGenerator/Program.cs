using System;
using System.Xml;

namespace PacketGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true, // 주석무시
                IgnoreWhitespace = true // 스페이스바 무시
                 
            };


            using (XmlReader r = XmlReader.Create("PDL.xml", settings))
            {
                r.MoveToContent(); // 헤더 문을 건너뛰고 핵심부분부터 진행함. <packet name = "PlayerInfoReq">문 부터 시작

                while(r.Read())
                {
                    if(r.Depth == 1 && r.NodeType == XmlNodeType.Element)
                    {
                        ParsePacket(r);
                    }

                    Console.WriteLine(r.Name + " " + r["name"]); // 타입 , 속성(어트리뷰트)
                }
            }


        }

        public static void ParsePacket(XmlReader r)
        {
            if(r.NodeType == XmlNodeType.EndElement)
            {
                return;
            }

            if(r.Name.ToLower() != "packet") //소문자 변환
            {
                Console.WriteLine("InValid Packet node");
                return;
            }

            string packetName = r["name"];
            if (string.IsNullOrEmpty(packetName))
            {
                Console.WriteLine("Packet without name");
                return;
            }

            ParseMembers(r);

        }

        public static void ParseMembers(XmlReader r) // XML 정보를 하나씩 긁어주는 역할을 하게됨.
        {
            string packetName = r["name"];

            int depth = r.Depth + 1; 
            while (r.Read())
            {
                if (r.Depth != depth)
                    break;

                string memberName = r["name"]; //r["name]은 실제 값을 의미
                if (string.IsNullOrEmpty(memberName))
                {
                    Console.WriteLine("Member without name");
                    return;
                }

                //r.Name은 어떤 타입인지 알수 있게 해준다.
                string memberType = r.Name.ToLower();

                switch (memberType)
                {
                    case "bool":
                    case "byte":
                    case "short":
                    case "int":
                    case "long":
                    case "float":
                    case "double":
                    case "string":
                    case "list":
                        break;
                    default:
                        break;
                }
            }
        }
    }
}