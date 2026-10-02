using RobotCombat.Domain.Communication.Transfer;
using Serilog;
using System.Net.Sockets;
using System.Text;

namespace RobotCombat.Domain.Communication
{
    /**
     * Classe responsable de la gestion de la connexion réseau avec le serveur
     */
    public class ConnectionHandler(Socket socket) : IDisposable
    {
        private static readonly ILogger Logger = Log.ForContext<SocketClient>();
        private const string Eom = "<|EOM|>";
        private bool stopped = false;

        /**
         * Vérifie si la connexion est toujours active
         */
        public bool IsConnected() => !stopped && socket.Connected;
        /**
         * Envoie un message au serveur, en ajoutant le marqueur de fin de message (EOM)
         */

        public async Task SendMessage(string message)
        {
            if (stopped)
            {
                Logger.Debug($"SendMessage ignoré, connexion arrêtée {socket.RemoteEndPoint}");
                return;
            }

            string content = message.ToString() + Eom;

            byte[] bytes = Encoding.UTF8.GetBytes(content);

            try
            {
                await socket.SendAsync(bytes, SocketFlags.None);
                Logger.Verbose($"{bytes.Length} octets envoyés à {socket.RemoteEndPoint}");

            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Échec d'envoi à {socket.RemoteEndPoint}");
                throw;
            }
        }

        private readonly StringBuilder pending = new();
        private readonly Decoder decoder = Encoding.UTF8.GetDecoder();

        /**
         * Réception des messages du serveur, en attente de la fin du message (EOM)
         */
        public async Task<string?> ReceiveMessage()
        {
            var buffer = new byte[4096];
            var chars = new char[Encoding.UTF8.GetMaxCharCount(buffer.Length)];
            int eomIndex = pending.ToString().IndexOf(Eom, StringComparison.Ordinal);

            while (eomIndex < 0)
            {
                if (stopped)
                {
                    return null;
                }
                int received;
                try
                {
                    received = await socket.ReceiveAsync(buffer, SocketFlags.None);
                }
                catch (Exception ex)
                {
                    Logger.Debug(ex, $"Erreur lors de la réception d'un message ({socket.RemoteEndPoint}, arrêtée : {stopped})");
                    throw;
                }
                if (received == 0)
                {
                    Logger.Debug($"{socket.RemoteEndPoint} a fermé la connexion");
                    return null;
                }

                int charCount = decoder.GetChars(buffer, 0, received, chars, 0);
                pending.Append(chars, 0, charCount);
                eomIndex = pending.ToString().IndexOf(Eom, StringComparison.Ordinal);
            }

            string message = pending.ToString(0, eomIndex);
            pending.Remove(0, eomIndex + Eom.Length);

            if (pending.Length > 0)
            {
                Logger.Information($"{pending.Length} caractères en attente");
            }

            return message;
        }

        /**
         * Libère les ressources utilisées
         */
        public void Dispose()
        {
            if (stopped)
            {
                return;
            }
            Logger.Debug($"Dispose de la connexion {socket.RemoteEndPoint}");

            stopped = true;

            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException ex)
            {
                Logger.Debug(ex, $"Shutdown ignoré ({socket.RemoteEndPoint})");
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Debug(ex, $"Socket déjà libéré ({socket.RemoteEndPoint})");
            }
            socket.Close();
            socket.Dispose();
        }

    }

}