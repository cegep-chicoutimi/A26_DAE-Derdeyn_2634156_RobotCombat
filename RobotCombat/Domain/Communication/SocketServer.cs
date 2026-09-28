using System.Net;
using System.Net.Sockets;

namespace RobotCombat.Domain.Communication
{
    public class SocketServer(int port) : ISocket
    {
        private bool isRunning;
        private Socket? listener;
        private ConnectionHandler? connection;
        private bool disposed = false;
        private  bool isClientConnected = false;
        /**
         *  Démarre le serveur et attend qu'un client se connecte.
         */
        public async Task Start()
        {
            if (listener == null)
            {
                StartServer();
                _ = AcceptLoop();
            }

            await WaitForClient();
        }

        public void StartServer()
        {
            IPEndPoint localIPEndPoint = new IPEndPoint(IPAddress.Any, port);
            Console.WriteLine($"Démarrage du serveur sur le port {port} avec l'ip {localIPEndPoint.Address}");

            listener = new Socket(localIPEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localIPEndPoint);
            listener.Listen(100);

            isRunning = true;
            disposed = false;

            Console.WriteLine("Serveur en attente de connexion...");
        }

        /**
         * Attend qu'un client se connecte.
         */
        public async Task WaitForClient()
        {
            while (isRunning && !isClientConnected)
            {
                await Task.Delay(100);
            }
        }

        /**
         * Accepte les connexions en continu.
         */
        private async Task AcceptLoop()
        {
            while (isRunning && listener != null)
            {
                Socket handler;
                try
                {
                    handler = await listener.AcceptAsync();
                }
                catch (Exception)
                {
                    break; // le serveur a été fermé (Exit)
                }

                if (!isClientConnected)
                {
                    connection = new ConnectionHandler(handler);
                    isClientConnected = true;
                }
                else
                {
                    using var clientRejected = new ConnectionHandler(handler);
                    try
                    {
                        await clientRejected.SendMessage("SERVER_BUSY");
                    }
                    catch (SocketException)
                    {
                    }
                }
            }
        }

        /**
         * Réception des messages du client.
         */
        public async Task<string?> Receive()
        {
            if (connection == null)
            {
                throw new InvalidOperationException("Aucun client connecté.");
            }

            if (!isRunning || !connection.IsConnected())
            {
                return null;
            }

            string? message;
            try
            {
                message = await connection.ReceiveMessage();
            }
            catch (SocketException)
            {
                message = null;
            }

            if (message == null)
            {
                // Le client est parti : la place est de nouveau libre
                connection.Dispose();
                connection = null;
                isClientConnected = false;
            }

            return message;
        }
        /**
         * Ferme le serveur et libère les ressources.
         */
        public void Exit()
        {
            if (disposed)
            {
                return;
            }

            isRunning = false;

            connection?.Dispose();
            connection = null;
            isClientConnected = false;

            listener?.Close();
            listener = null;

            disposed = true;
        }

        /**
         * Envoie un message au client connecté.
         */
        public async Task Send(string message)
        {
            if (connection != null && connection.IsConnected())
            {
                await connection.SendMessage(message);
            }
        }
        /**
         * Vérifie si un client est connecté.
         */
        public bool IsConnected() => isClientConnected;
    }
}