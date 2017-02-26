using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace CustomTCPProtocoll
{
    [Serializable]
    public class TCP_Protocoll
    {
        /// <summary>
        /// Serialisiert eine Klasse damit sie als String versendet werden kann
        /// </summary>
        /// <param name="protocol">Eine Instanz der Klasse TCP_Protocol</param>
        /// <returns>String fürs netzwerk senden</returns>
        private static byte[] generateStringFromProtocol(TCP_Protocoll protocol)
        {
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, protocol);
            stream.Position = 0;

            byte[] buffer = new byte[stream.Length];

            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Deserialisert eine Klasse damit sie als Object verwendet werden kann
        /// </summary>
        /// <param name="protocol">String aus dem Netzwerk Read</param>
        /// <returns>Eine Instanz der TCP_Protocol Klasse</returns>
        private static TCP_Protocoll generateProtocolFromString(byte[] protocol)
        {
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream(protocol);
            return (TCP_Protocoll)formatter.Deserialize(stream);
        }

        /// <summary>
        /// Sends a Message over the Network
        /// </summary>
        /// <param name="protocol">Instance of a TCP_Protocoll with Stored-Data</param>
        /// <param name="Writer">Binary Writer for network Communication</param>
        /// <returns>True for sucess, False for error</returns>
        public static bool SendMessage(TCP_Protocoll protocol, BinaryWriter Writer, Dictionary<int, string> Dictionary)
        {
            try
            {
                if (Dictionary != null)
                {
                    protocol.ProtocolType = CreateHeaderForWrite(protocol.ProtocolType, Dictionary); 
                }

                byte[] toSend = generateStringFromProtocol(protocol);
                Writer.Write((Int32)toSend.GetLength(0));
                Writer.Write(toSend);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Reads a Message from an Input-Stream
        /// </summary>
        /// <param name="Reader">Binary Reader for Network Communcation</param>
        /// <returns>TCP_Protocoll with Data</returns>
        public static TCP_Protocoll ReadMessage(BinaryReader Reader, Dictionary<int, string> Dictionary)
        {
            try
            {
                Int32 readLength = Reader.ReadInt32();
                TCP_Protocoll p = TCP_Protocoll.generateProtocolFromString(Reader.ReadBytes(readLength));
                if (Dictionary != null)
                {
                    p.ProtocolType = CreateHeaderFromAwnswer(p.ProtocolType, Dictionary); 
                }
                return p;
            }
            catch
            {
                return new TCP_Protocoll(HeaderData.Error, "");
            }
        }

        /// <summary>
        /// Creates a Dictionary with needed definitions
        /// </summary>
        /// <param name="DicDiffSeed"></param>
        /// <returns></returns>
        public static Dictionary<int, string> CreateDicForComm(int DicDiffSeed)
        {
            Dictionary<int, string> ret = new Dictionary<int, string>();
            Random rnd = new Random(DicDiffSeed);

            foreach (string EnumValue in Enum.GetNames(typeof(HeaderData)))
            {
                int rndNumber = rnd.Next(1, 2147483647);

                if (!ret.ContainsKey(rndNumber))
                {
                    ret.Add(rndNumber, EnumValue);
                }
            }

            return ret;
        }

        /// <summary>
        /// Creates the new Header for Wriring to the Socket!
        /// </summary>
        /// <param name="Header">Original Header</param>
        /// <param name="Dic">Translate Dictionary</param>
        /// <returns>New Header</returns>
        private static HeaderData CreateHeaderForWrite(HeaderData Header, Dictionary<int, string> Dic)
        {
            if ((Dic != null)&(Header != HeaderData.DictonaryDefinitionsSeed))
            {
                int randomHeaderId = Dic.FirstOrDefault(x => x.Value == Enum.GetName(typeof(HeaderData), Header)).Key;
                return (HeaderData)randomHeaderId; 
            }
            else
            {
                return Header;
            }
        }

        /// <summary>
        /// Creates the new Header from the Writing of the Socket!
        /// </summary>
        /// <param name="Header">New Header</param>
        /// <param name="Dic">Translate Dictionary</param>
        /// <returns>Original Header</returns>
        private static HeaderData CreateHeaderFromAwnswer(HeaderData Header, Dictionary<int, string> Dic)
        {
            if ((Dic != null)&(Header != HeaderData.DictonaryDefinitionsSeed))
            {
                string EnumName = Dic.FirstOrDefault(x => x.Key == (int)Header).Value;
                return (HeaderData)Enum.Parse(typeof(HeaderData), EnumName); 
            }
            else
            {
                return Header;
            }
        }

        /// <summary>
        /// Enum Header for Communication Easy-Definition
        /// </summary>
        public enum HeaderData
        {
            Hello = 1,
            Error = 2,
            CustomData = 20,
            DictonaryDefinitionsSeed = 40,
            Broadcast = 100,
            GoodBye = 2147483647
        }

        /// <summary>
        /// Type of Telegram
        /// </summary>
        public HeaderData ProtocolType;
        /// <summary>
        /// Data that will be / has been sendet
        /// </summary>
        public object Data;

        /// <summary>
        /// Constuctor for TCP-Data
        /// </summary>
        /// <param name="ProtocolType"></param>
        /// <param name="Data"></param>
        public TCP_Protocoll(HeaderData ProtocolType, object Data)
        {
            this.ProtocolType = ProtocolType;
            this.Data = Data;
        }
    }
}
