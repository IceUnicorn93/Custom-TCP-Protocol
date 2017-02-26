using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace CustomTCPProtocoll
{
    public class ClientClass
    {
        public delegate void antwortHandler(TCP_Protocoll Antwort);
        public event antwortHandler Geantwortet;

        private Socket socket;
        private Stream myStream;
        private BinaryReader reader;
        private BinaryWriter writer;
        private Thread RunThread;

        private Dictionary<int, string> TranslationDictionary = null;

        public bool IsConnected { get { return this.socket.Connected; } }

        public ClientClass()
        {
            this.socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

        public bool Connect(string Host, int Port)
        {
            try
            {
                IPAddress IPA;

                if (!IPAddress.TryParse(Host, out IPA))
                {   // Hostname
                    IPAddress[] ips;
                    ips = Dns.GetHostAddresses(Host);
                    foreach (IPAddress myIP in ips)
                    {
                        if (Connect(myIP.ToString(), Port))
                        {
                            return this.socket.Connected;
                        }
                    }
                }

                this.socket.Connect(new IPEndPoint(IPA, Port));

                this.myStream = new NetworkStream(socket);
                this.reader = new BinaryReader(myStream);
                this.writer = new BinaryWriter(myStream);

                this.RunThread = new Thread(new ThreadStart(ThreadRun));
                this.RunThread.Start();

                return this.socket.Connected;
            }
            catch
            {
                return false;
            }
        }

        private void ThreadRun()
        {
            bool stopToken = false;
            while ((this.socket.Connected) && (stopToken == false))
            {
                Thread.Sleep(10);
                if (this.socket.Available > 0)
                {
                    TCP_Protocoll antwort = TCP_Protocoll.ReadMessage(this.reader, TranslationDictionary);
                    
                    if (antwort.ProtocolType == TCP_Protocoll.HeaderData.DictonaryDefinitionsSeed)
                        TranslationDictionary = TCP_Protocoll.CreateDicForComm((int)antwort.Data);

                    if (antwort.ProtocolType == TCP_Protocoll.HeaderData.GoodBye)
                        stopToken = true;

                    if ((this.Geantwortet != null)&(TranslationDictionary != null))
                        this.Geantwortet(antwort);
                }
            }
            this.socket.Close(); // Maybe this creats erros, may move to the end of while
        }

        public void Write(TCP_Protocoll.HeaderData Header, object Data)
        {
            TCP_Protocoll.SendMessage(new TCP_Protocoll(Header, Data), this.writer, this.TranslationDictionary);
        }
    }
}
