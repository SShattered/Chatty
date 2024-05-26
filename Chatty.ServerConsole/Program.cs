using Chatty.Server;

namespace Chatty.ServerConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TcpServer server = new TcpServer();
            server.Start();
            server.ClientValidating += Server_ClientValidating;
            server.ClientDisconnected += Server_ClientDisconnected;
            Console.Read();
        }

        private static void Server_ClientDisconnected(Receiver receiver)
        {
            Console.WriteLine($"Client disconnected!");
        }

        private static void Server_ClientValidating()
        {
            throw new NotImplementedException();
        }

        private static void Server_ClientConnected(Receiver receiver)
        {
            Console.WriteLine($"Client connected!");
        }
    }
}
