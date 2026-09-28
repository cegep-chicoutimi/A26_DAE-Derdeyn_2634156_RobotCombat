using System.Net;
using System.Net.Sockets;

namespace RobotCombat.Domain.Communication
{
    /**
     * Classe représentant un client socket pour la communication avec un serveur.
     */
    public class SocketClient(string ipAddress, int port) : ISocket
    {
        private const string BusyMessage = "SERVER_BUSY";
        private ConnectionHandler? connection;

        public async Task Send(string message)
        {
            if (connection == null)
            {
                return;
            }

            await connection.SendMessage(message);
        }

        public async Task<string?> Receive()
        {
            if (connection == null)
            {
                return null;
            }

            string? message = await connection.ReceiveMessage();

            if (message == BusyMessage)
            {
                Console.WriteLine("Le serveur est à sa capacité maximale.");
                connection.Dispose();
                connection = null;
                return null;
            }

            return message;
        }

        public void Exit()
        {
            if (connection != null)
            {
                connection.Dispose();
                connection = null;
            }

            Console.WriteLine("Client déconnecté.");
        }

        public Task Start() => ConnectToServer();

        public bool IsConnected() => connection?.IsConnected() ?? false;

        private async Task ConnectToServer()
        {
            IPEndPoint remoteEndPoint = new(IPAddress.Parse(ipAddress), port);

            Socket socket = new(remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true
            };

            try
            {
                await socket.ConnectAsync(remoteEndPoint);
            }
            catch
            {
                socket.Dispose();
                throw; 
            }

            connection = new ConnectionHandler(socket);
        }
    }
}