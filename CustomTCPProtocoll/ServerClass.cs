using System;

using CustomTCPProtocoll.ServerClasses;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace CustomTCPProtocoll
{
    public class ServerClass
    {
        public event antwortHandle ClientAntwort;

        private tokenInformation myServerTokenInformation;
        private Thread myClientAccept;
        private Thread myUpdateThread;

        private int Port;

        public bool Running
        {
            get
            {
                return !myServerTokenInformation.Stop;
            }
        }

        public ServerClass(int ServerPort)
        {
            this.Port = ServerPort;
            this.myServerTokenInformation = new tokenInformation();
            this.myServerTokenInformation.Port = Port;

            this.myClientAccept = new Thread(new ParameterizedThreadStart(this.threadAcceptClients));
            this.myUpdateThread = new Thread(new ParameterizedThreadStart((object o) =>
            {
                tokenInformation myToken = (tokenInformation)o;
                while (myToken.Stop == false)
                {
                    Console.Title = String.Format("Server is running on Port: {0} with {1} Client(s)", myToken.Port, ServerClient.ConnectedClientsCount);
                    Thread.Sleep(10000);
                }
            }));
        }

        private void threadAcceptClients(object myTokenInformation)
        {
            tokenInformation mTI = (tokenInformation)myTokenInformation;

            Socket serverSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);

            serverSocket.Bind(new IPEndPoint(IPAddress.Any, mTI.Port));

            serverSocket.Listen(0);

            while (!mTI.Stop)
            {
                Socket clientSocket = serverSocket.Accept();

                if (clientSocket != null)
                {
                    ServerClient c = new ServerClient(clientSocket);
                    c.Geantwortet += this.Client_Antwort;
                    c.SendDDS();
                }
            }
        }

        private void Client_Antwort(ServerClient Client, TCP_Protocoll Antwort)
        {
            if (this.ClientAntwort != null)
            {
                this.ClientAntwort(Client, Antwort);
            }
        }

        public void Start()
        {
            this.myUpdateThread.Start((object)this.myServerTokenInformation);
            this.myClientAccept.Start((object)this.myServerTokenInformation);

            while (this.myServerTokenInformation.Stop == false)
            {
                Console.WriteLine("To stop the Server press ESC");
                ConsoleKeyInfo cki = Console.ReadKey();
                if (cki.Key == ConsoleKey.Escape)
                {
                    this.Stop();
                }
            }
        }

        public void Stop()
        {
            this.myServerTokenInformation.Stop = true;
        }
    }
}
