using RobotCombat.Domain.Communication.Transfer;
using RobotCombat.Domain.Game;
using Serilog;
using System.Net;
using System.Net.Sockets;

namespace RobotCombat.Domain.Communication
{
    /**
     * Classe représentant un client socket pour la communication avec un serveur.
     */
    public class SocketClient(string ipAddress, int port) : ISocket
    {
        private static readonly ILogger Logger = Log.ForContext<SocketClient>();
        private static string BusyMessage = MessageHelper.BuildMessage(MessageType.SERVER_BUSY, null, GameStatus.WAITING_FOR_PLAYER, "");
        private ConnectionHandler? connection;

        /**
         * Envoie un message au serveur via le socket.
         */
        public async Task Send(string message)
        {
            if (connection == null)
            {
                Logger.Information($"Message envoyé dans le vide car déconnecté { message}");
                return;
                
            }
            Logger.Debug($">> Envoi vers le serveur: {message}");
            await connection.SendMessage(message);
        }
        /**
         * Reçoit un message du serveur via le socket.
         */
        public async Task<string?> Receive()
        {
            if (connection == null)
            {
                Logger.Debug("En attente de message mais aucune connexion");
                return null;
            }

            string? message = await connection.ReceiveMessage();

            if (message == null)
            {
                Logger.Information("Connexion fermée par le serveur (ou arrêtée)");
                return null;
            }

            Logger.Debug($"<< Message reçu : {message}");

            if (message == BusyMessage)
            {
                Logger.Warning("Le serveur est full, fermeture de la connexion");
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
            Logger.Information($"Fermeture du socket demandé (connexion active : {connection != null})");
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

            Logger.Information($"Tentative de connexion à {remoteEndPoint}");

            try
            {
                await socket.ConnectAsync(remoteEndPoint);
            }
            catch(Exception ex)
            {
                Logger.Error(ex, "Échec de connexion à {remoteEndPoint}");
                socket.Dispose();
                throw;
            }

            Logger.Information($"Connecté à {remoteEndPoint}");
            connection = new ConnectionHandler(socket);
        }
    }
}