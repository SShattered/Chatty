using Chatty.Shared;
using System.Net.Sockets;

namespace Chatty.Server
{
    public class TcpServer
    {
        public List<Receiver> receivers;

        public readonly List<ResponseCallbackObject> _callBacks;

        private TcpListener listener;
        private bool isStarted = false;

        public event Delegates.ClientValidatingDelegate ClientValidating;
        public event Delegates.ClientBasicDelegate ClientConnected;
        public event Delegates.ClientBasicDelegate ClientValidated;
        public event Delegates.ClientBasicDelegate ClientDisconnected;

        public TcpServer()
        {
            listener = new TcpListener(System.Net.IPAddress.Any, 8888);
            _callBacks = new List<ResponseCallbackObject>();
            receivers = new List<Receiver>();
        }

        public void Start()
        {
            if (!isStarted)
            {
                listener.Start();
                listener.BeginAcceptTcpClient(new AsyncCallback(ConnectionHandler), null);
            }
        }

        public void Stop() 
        { 
            if (isStarted)
                listener.Stop();
        }

        private void ConnectionHandler(IAsyncResult asyncResult)
        {
            Receiver receiver = new Receiver(listener.EndAcceptTcpClient(asyncResult), this);
            receiver.Disconnected += Receiver_ClientDisconnected;
            OnClientConnected(receiver);
            receivers.Add(receiver);
            listener.BeginAcceptTcpClient(new AsyncCallback(ConnectionHandler), null);
        }

        private void Receiver_ClientDisconnected(Receiver receiver)
        {
            Console.WriteLine($"Client disconnected");
        }

        public virtual void OnClientConnected(Receiver receiver)
        {
            Console.WriteLine($"Client connected - {receiver.GetGuid()}");
            ClientConnected?.Invoke(receiver);
        }

        public virtual void OnClientDisconnected(Receiver receiver)
        {
            ClientDisconnected?.Invoke(receiver);
        }
    }
}