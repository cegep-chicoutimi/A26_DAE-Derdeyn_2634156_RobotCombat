using RobotCombat.Domain.Communication.Transfer;
using Serilog;
using System.Net;
using System.Net.Sockets;

namespace RobotCombat.Domain.Communication
{
    public class SocketServer(int port, string ip) : ISocket
    {
        private static readonly ILogger Logger = Log.ForContext<SocketServer>();
        private bool isRunning;
        private Socket? listener;
        private ConnectionHandler? connection;
        private bool disposed = false;
        private  bool isClientConnected = false;
        /// <summary>
        ///  Démarre le serveur et attend qu'un client se connecte.
        /// </summary>
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

            listener = new Socket(localIPEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localIPEndPoint);
            listener.Listen(100);
            Logger.Information($"Serveur démarré sur le port {port}");
            isRunning = true;
            disposed = false;
        }

        /// <summary>
        /// Attend qu'un client se connecte.
        /// </summary>
        public async Task WaitForClient()
        {
            while (isRunning && !isClientConnected)
            {
                await Task.Delay(100);
            }
        }

        /// <summary>
        /// Accepte les connexions en continu.
        /// </summary>
        private async Task AcceptLoop()
        {
            while (isRunning && listener != null)
            {
                Socket handler;
                try
                {
                    handler = await listener.AcceptAsync();
                }
                catch (Exception ex)
                {
                    Logger.Debug(ex, "AcceptLoop arrêtée (serveur arrêté)");
                    break; // le serveur a été fermé (Exit)
                }

                if (!isClientConnected)
                {
                    connection = new ConnectionHandler(handler);
                    isClientConnected = true;
                    Logger.Information($"Client connecté : {handler.RemoteEndPoint}");
                }
                else
                {
                    using var clientRejected = new ConnectionHandler(handler);
                    try
                    {
                        Logger.Information("Client rejeté, serveur complet");
                        await clientRejected.SendMessage(MessageHelper.BuildMessage(MessageType.SERVER_BUSY, null));
                    }
                    catch (SocketException ex)
                    {
                        Logger.Debug(ex, "Serveur complet, Impossible d'envoyer SERVER_BUSY");
                    }
                }
            }
        }

        /// <summary>
        /// Réception des messages du client.
        /// </summary>
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
            catch (SocketException ex)
            {
                Logger.Warning(ex, "Erreur socket pendant la réception");
                message = null;
            }

            if (message == null)
            {
                Logger.Information("Client déconnecté, place libérée");
                // Le client est parti : la place est de nouveau libre
                connection.Dispose();
                connection = null;
                isClientConnected = false;
            }
            else
            {
                Logger.Information($"Message reçu : {message}");
            }

            return message;
        }
        /// <summary>
        /// Ferme le serveur et libère les ressources.
        /// </summary>
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

        /// <summary>
        /// Envoie un message au client connecté.
        /// </summary>
        public async Task Send(string message)
        {
            Logger.Debug($">> Message envoyé : {message}");
            if (connection != null && connection.IsConnected())
            {
                await connection.SendMessage(message);
            } else
            {
                Logger.Warning("Envoi ignoré, aucune connexion active : {Message}", message);
            }
        }
        /// <summary>
        /// Vérifie si un client est connecté.
        /// </summary>
        public bool IsConnected() => isClientConnected;
    }
}