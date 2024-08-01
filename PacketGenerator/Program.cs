using System;
using System.Security.Principal;
using System.Xml;

namespace PacketGenerator
{
    class Program
    {
        /// <summary>
        /// PDL.xml을 읽고, 패킷을 정의하는 클래스를 정의합니다.
        /// </summary>
        static string getPackets; // 실시간으로 만들어 지는 패킷스트링을 만들어서 관리 
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
                    File.WriteAllText("GenPackets.cs", getPackets);
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

            Tuple<string, string, string>  t = ParseMembers(r);
            getPackets += string.Format(PacketFormat.packetFormat, packetName, t.Item1 , t.Item2 , t.Item3);

        }

        //{1} 멤버 변수
        //{2} 멤버 변수 Read
        //{3} 멤버 변수 Write

        public static Tuple<string, string, string> ParseMembers(XmlReader r) // XML 정보를 하나씩 긁어주는 역할을 하게됨.
        {
            string packetName = r["name"];

            string membercode = "";
            string readcode = "";
            string writecode = "";

            int depth = r.Depth + 1; 
            while (r.Read())
            {
                if (r.Depth != depth)
                    break;

                string memberName = r["name"]; //r["name]은 실제 값을 의미
                if (string.IsNullOrEmpty(memberName))
                {
                    Console.WriteLine("Member without name");
                    return null;

                }

                if (string.IsNullOrEmpty(membercode) == false)
                {
                    membercode += Environment.NewLine; // 엔터를 치는 것과 동일
                }

                if (string.IsNullOrEmpty(readcode) == false)
                {
                    readcode += Environment.NewLine; // 엔터를 치는 것과 동일
                }

                if (string.IsNullOrEmpty(writecode) == false)
                {
                    writecode += Environment.NewLine; // 엔터를 치는 것과 동일
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
                        membercode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readcode += string.Format(PacketFormat.readFormat, memberName, ToMemberType(memberType), memberType);
                        writecode += string.Format(PacketFormat.writeFormat, memberName, memberType);
                        break;
                    case "string":
                        membercode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readcode += string.Format(PacketFormat.readStringFormat, memberName);
                        writecode += string.Format(PacketFormat.writeStringFormat, memberName);
                        break;
                    case "list":
                        Tuple<string, string, string> t = ParseList(r);
                        membercode += t.Item1;
                        readcode += t.Item2;
                        writecode += t.Item3;

                        break;
                    default:
                        break;
                }
            }

            membercode = membercode.Replace("\n", "\n\t");
            readcode = readcode.Replace("\n", "\n\t\t");
            writecode = writecode.Replace("\n", "\n\t\t");
            return new Tuple<string, string, string>(membercode, readcode, writecode);
        }

        public static Tuple<string, string, string> ParseList(XmlReader r)
        {
            string listname = r["name"];
            if(string.IsNullOrEmpty(listname))
            {
                Console.WriteLine("List without name");
                return null;
            }

            Tuple<string, string, string> t = ParseMembers(r);

            string membercode = string.Format(PacketFormat.memberListFormat,
                FirstCharToUpper(listname), FirstCharToLower(listname),
                t.Item1,
                t.Item2,
                t.Item3);

            string readcode = string.Format(PacketFormat.readListFormat,
               FirstCharToUpper(listname), FirstCharToLower(listname));

            string writecode = string.Format(PacketFormat.writeListFormat,
              FirstCharToUpper(listname), FirstCharToLower(listname));



            return new Tuple<string, string, string>(membercode, readcode, writecode);


        }

        public static string ToMemberType(string memberType)
        {
            switch(memberType)
            {
                case "bool":
                    return "ToBoolean";             
                case "short":
                    return "ToInt16";
                case "int":
                    return "ToInt32";
                case "long":
                    return "ToUInt64";
                case "float":
                    return "ToSingle";
                case "double":
                    return "ToDouble";
                default:
                    return "";
            }
        }

        public static string FirstCharToUpper(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            return input[0].ToString().ToUpper() + input.Substring(1);
        }

        public static string FirstCharToLower(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            return input[0].ToString().ToLower() + input.Substring(1);
        }
    }
}