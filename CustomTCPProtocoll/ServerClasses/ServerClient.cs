using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace CustomTCPProtocoll.ServerClasses
{
    public delegate void antwortHandle(ServerClient Client, TCP_Protocoll Antwort);
    public class ServerClient
    {
        public event antwortHandle Geantwortet;

        public static int ConnectedClientsCount
        {
            get { return _list.Count; }
        }
        private static int _NextClientID = 0;
        public static int NextClientID { get { _NextClientID++; return _NextClientID; } }
        public int ClientID { get; private set; }

        private static LinkedList<ServerClient> _list = new LinkedList<ServerClient>();

        private Socket _socket;
        private NetworkStream _stream;
        private BinaryWriter _outStream;
        private BinaryReader _inpStream;
        private Thread _updateTread;
        private tokenInformation _token;

        private Dictionary<int, string> TranslationDictionary = null;

        private BinaryWriter Writer
        {
            get { return this._outStream; }
        }

        public static void SendToAllClients(TCP_Protocoll protocol, int SenderID)
        {
            try
            {
                foreach (ServerClient cli in _list)
                {
                    if (cli.ClientID != SenderID)
                    {
                        cli.Write(protocol);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("EXCEPTION");
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public ServerClient(Socket mySocket)
        {
            this._socket = mySocket;
            this._stream = new NetworkStream(this._socket);
            this._inpStream = new BinaryReader(this._stream);
            this._outStream = new BinaryWriter(this._stream);
            this._token = new tokenInformation();
            this._updateTread = new Thread(new ParameterizedThreadStart((object o) =>
            {
                tokenInformation mTI = (tokenInformation)o;
                while (!mTI.Stop)
                {
                    this.Update();
                }
            }));
            _list.AddLast(this);

            ClientID = ServerClient.NextClientID;

            _updateTread.Start(_token);
        }

        public void SendDDS()
        {
            int randomNumber = new Random().Next(0, Int32.MaxValue);
            this.TranslationDictionary = TCP_Protocoll.CreateDicForComm(randomNumber);
            this.Write(new TCP_Protocoll(TCP_Protocoll.HeaderData.DictonaryDefinitionsSeed, randomNumber));
        }

        public void Write(TCP_Protocoll protocol)
        {
            TCP_Protocoll.SendMessage(protocol, this.Writer, this.TranslationDictionary);
        }

        public void Disconnect()
        {
            this._token.Stop = true;
            this._stream.Close();
            this._socket.Close();
            _list.Remove(this);
        }

        public void Update()
        {
            Thread.Sleep(10);
            if (!this._socket.Connected)
            {
                _list.Remove(this);
                return;
            }

            if (this._socket.Available > 0)
            { 
                TCP_Protocoll antwort = TCP_Protocoll.ReadMessage(this._inpStream, this.TranslationDictionary);

                if (antwort.ProtocolType == TCP_Protocoll.HeaderData.GoodBye)
                {
                    Console.WriteLine("Client {0} says Goodbye. " + antwort.Data, this.ClientID);
                    Disconnect();
                }

                if (antwort.ProtocolType == TCP_Protocoll.HeaderData.DictonaryDefinitionsSeed)
                {
                    this.TranslationDictionary = TCP_Protocoll.CreateDicForComm((int)antwort.Data);
                }

                if (this.Geantwortet != null)
                {
                    this.Geantwortet(this, antwort);
                }
            }
        }
    }
}
