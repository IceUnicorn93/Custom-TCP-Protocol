using System;
using System.Threading;
using CustomTCPProtocoll;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            ClientClass client = new ClientClass();
            client.Geantwortet += Client_Geantwortet;
            client.Connect("localhost", 15321);

            Thread t = new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(1000);
                while (client.IsConnected)
                {
                    client.Write(TCP_Protocoll.HeaderData.Broadcast, "Hello From " + (new Random()).Next(0, 100).ToString());
                    Thread.Sleep(1000);
                }
            }));
            t.Start();

            Console.ReadKey();
        }

        private static void Client_Geantwortet(TCP_Protocoll Antwort)
        {
            switch (Antwort.ProtocolType)
            {
                #region Hello Message
                case TCP_Protocoll.HeaderData.Hello:
                    Console.WriteLine("Hello Message from Server: " + Antwort.Data);
                    break;
                #endregion
                #region Error Message
                case TCP_Protocoll.HeaderData.Error:
                    Console.WriteLine("Error in Communication");
                    break;
                #endregion
                #region CustomData Message
                case TCP_Protocoll.HeaderData.CustomData:
                    Console.WriteLine("CusomtData from Server: " + Antwort.Data);
                    break;
                #endregion
                #region Broadcast Message
                case TCP_Protocoll.HeaderData.Broadcast:
                    Console.WriteLine("Broadcast from Server: " + Antwort.Data);
                    break;
                #endregion
                #region DictonaryDef Message
                case TCP_Protocoll.HeaderData.DictonaryDefinitionsSeed:
                    Console.WriteLine("Dictonary Definitions Seed: " + Antwort.Data);
                    break;
                #endregion
                #region Goodbye Message
                case TCP_Protocoll.HeaderData.GoodBye:
                    Console.WriteLine("Implement Your Def. here!");
                    break;
                #endregion
                default:
                    Console.WriteLine("Hey Dev.! You realy should implement the following Enum!");
                    Console.WriteLine(Enum.GetName(typeof(TCP_Protocoll.HeaderData), Antwort.ProtocolType));
                    break;
            }
        }
    }
}
