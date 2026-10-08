using RobotCombat.Domain.Communication.Transfer;
using RobotCombat.Domain.Game;
using Serilog;
using System.Net;
using System.Net.Sockets;

namespace RobotCombat.Domain.Communication
{
    /// <summary>
    /// Classe représentant un client socket pour la communication avec un serveur.
    /// </summary>
    public class SocketClient(string ipAddress, int port) : ISocket
    {
        private static readonly ILogger Logger = Log.ForContext<SocketClient>();
        private ConnectionHandler? connection;

        /// <summary>
        /// Envoie un message au serveur via le socket.
        /// </summary>
        public async Task Send(string message)
        {
            if (connection == null)
            {
                Logger.Information($"Message envoyé dans le vide car déconnecté { message}");
                return;
                
            }
            Logger.Debug($">> Envoi vers le serveur: {message}");
            try
            {
                await connection.SendMessage(message);
            }
            catch (Exception ex)
            {
                // Le serveur a coupé la connexion
                Logger.Warning(ex, "Envoi impossible, connexion fermée par le serveur");
                Exit();
            }
        }
        /// <summary>
        /// Reçoit un message du serveur via le socket.
        /// </summary>
        public async Task<string?> Receive()
        {
            if (connection == null)
            {
                Logger.Debug("En attente de message mais aucune connexion");
                return null;
            }

            string? message;
            try
            {
                message = await connection.ReceiveMessage();
            }
            catch (Exception ex)
            {
                // Le serveur a coupé la connexion
                Logger.Warning(ex, "Connexion fermée par le serveur");
                Exit();
                return null;
            }

            if (message == null)
            {
                Logger.Information("Connexion fermée par le serveur (ou arrêtée)");
                return null;
            }

            Logger.Debug($"<< Message reçu : {message}");
            return message;
        }
        /// <summary>
        /// Ferme la connexion avec le serveur.
        /// </summary>
        public void Exit()
        {
            Logger.Information($"Fermeture du socket demandé (connexion active : {connection != null})");
            if (connection != null)
            {
                connection.Dispose();   
                connection = null;
            }

        }
        /// <summary>
        /// Démarre la connexion avec le serveur.
        /// </summary>
        public Task Start() => ConnectToServer();

        /// <summary>
        /// Vérifie si le client est connecté au serveur.
        /// </summary>
        public bool IsConnected() => connection?.IsConnected() ?? false;
        /// <summary>
        /// Établit une connexion avec le serveur.
        /// </summary>
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