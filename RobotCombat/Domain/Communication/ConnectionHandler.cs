using RobotCombat.Domain.Communication.Transfer;
using System.Net.Sockets;
using System.Text;

namespace RobotCombat.Domain.Communication
{
    /**
     * Classe responsable de la gestion de la connexion réseau avec le serveur
     */
    public class ConnectionHandler(Socket socket) : IDisposable
    {
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
                return;
            }

            string content = message.ToString() + Eom;

            byte[] bytes = Encoding.UTF8.GetBytes(content);

            await socket.SendAsync(bytes, SocketFlags.None);
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
            int eomIndex = pending.ToString().IndexOf(Eom);

            while (eomIndex < 0)
            {
                if (stopped)
                {
                    return null;
                }

                int received = await socket.ReceiveAsync(buffer, SocketFlags.None);
                if (received == 0)
                {
                    return null;   // le socket a fermé la connexion
                }

                int charCount = decoder.GetChars(buffer, 0, received, chars, 0);
                pending.Append(chars, 0, charCount);
                eomIndex = pending.ToString().IndexOf(Eom);
            }

            string message = pending.ToString(0, eomIndex);
            pending.Remove(0, eomIndex + Eom.Length);
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

            stopped = true;

            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            socket.Close();
            socket.Dispose();
        }

    }

}