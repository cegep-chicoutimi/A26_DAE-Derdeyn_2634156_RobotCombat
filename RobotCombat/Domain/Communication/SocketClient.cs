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

        /**
         * Envoie un message au serveur via le socket.
         */
        public async Task Send(string message)
        {
            if (connection == null)
            {
                return;
            }

            await connection.SendMessage(message);
        }
        /**
         * Reçoit un message du serveur via le socket.
         */
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
        /**
         * Ferme la connexion avec le serveur.
         */
        public void Exit()
        {
            if (connection != null)
            {
                connection.Dispose();
                connection = null;
            }

        }
        /**
         * Démarre la connexion avec le serveur.
         */
        public Task Start() => ConnectToServer();

        /**
         * Vérifie si le client est connecté au serveur.
         */
        public bool IsConnected() => connection?.IsConnected() ?? false;
        /**
         * Établit une connexion avec le serveur.
         */
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