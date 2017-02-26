using System;
using CustomTCPProtocoll;
using CustomTCPProtocoll.ServerClasses;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerClass sc = new ServerClass(15321);

            sc.ClientAntwort += ClientAntwort;

            sc.Start();

            while (sc.Running == true)
            {
                Console.WriteLine("To stop the Server press ESC");
                ConsoleKeyInfo cki = Console.ReadKey();
                if (cki.Key == ConsoleKey.Escape)
                {
                    sc.Stop();
                }
            }

            Console.WriteLine("Press any Key to close the Program. . .");
            Console.ReadKey();
        }

        private static void ClientAntwort(ServerClient Client, TCP_Protocoll Antwort)
        {
            Console.WriteLine("##--##--##--##--##--##--##--##--##--##--##");
            string EnumName = Enum.GetName(typeof(TCP_Protocoll.HeaderData), Antwort.ProtocolType);
            switch (Antwort.ProtocolType)
            {
                case TCP_Protocoll.HeaderData.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("ERROR");
                    Console.WriteLine("Client ID: " + Client.ClientID.ToString());
                    Console.WriteLine("Type: " + EnumName);
                    Console.WriteLine("Int-Type: " + ((int)Antwort.ProtocolType).ToString());
                    Console.WriteLine("Message: " + Antwort.Data);
                    Console.ForegroundColor = ConsoleColor.White;
                    Client.Disconnect();
                    break;
                case TCP_Protocoll.HeaderData.Broadcast:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("BROADCAST");
                    ServerClient.SendToAllClients(Antwort, Client.ClientID);
                    Console.WriteLine("Client ID: " + Client.ClientID.ToString());
                    Console.WriteLine("Type: " + EnumName);
                    Console.WriteLine("Message: " + Antwort.Data);
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("Client ID: " + Client.ClientID.ToString());
                    Console.WriteLine("Type: " + EnumName);
                    Console.WriteLine("Message: " + Antwort.Data);
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }
    }
    }
}
