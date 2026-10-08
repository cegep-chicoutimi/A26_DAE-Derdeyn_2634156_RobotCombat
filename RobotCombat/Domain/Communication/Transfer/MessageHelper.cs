using RobotCombat.Domain.Game;
using System.Text.Json;

namespace RobotCombat.Domain.Communication.Transfer
{
    /// <summary>
    /// Classe utilitaire pour la construction et l'analyse des messages échangés entre le client et le serveur.
    /// </summary>
    public class MessageHelper
    {
       private static readonly JsonSerializerOptions JsonSerializerOption = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        /// <summary>
        /// Construit un message JSON à partir des paramètres fournis.
        /// </summary>
        /// <param name="type">Le type de message.</param>
        /// <param name="action">L'action du robot associée (peut être null).</param>
        /// <param name="status">Le statut de la partie.</param>
        /// <param name="data">Les données du message.</param>
        /// <returns>Une chaîne JSON représentant le message.</returns>
        public static string BuildMessage(MessageType type,  GameAction? action, GameStatus status = GameStatus.WAITING_FOR_PLAYER, string data = "")
        {
            var message =  new Message
            {
                Type = type,
                Action = action,
                Status = status,
                Data = data
            };
            return JsonSerializer.Serialize(message, JsonSerializerOption);
           
        }

        /// <summary>
        /// Convertit une chqîne de caractère au format JSON en un objet Message.
        /// </summary>
        /// <param name="message">La chaîne de caractère représentant le message.</param>
        /// <returns>L'objet Message correspondant.</returns>
        /// <exception cref="SerializeException">Si la désérialisation échoue.</exception>
        public static Message ParseMessage(string message)
        {
            try
            {
                return JsonSerializer.Deserialize<Message>(message, JsonSerializerOption);
            }
            catch (Exception ex)
            {
                throw new SerializeException("Impossible de désérialiser le message au format JSON.", ex);
            }

        }
    }
}
