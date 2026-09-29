using RobotCombat.Domain.Game;
using System.Text.Json;

namespace RobotCombat.Domain.Communication.Transfer
{
    /**
     * Classe utilitaire pour la construction et l'analyse des messages échangés entre le client et le serveur.
     */
    public class MessageHelper
    {
       private static readonly JsonSerializerOptions JsonSerializerOption = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        /**
         * Construit un message JSON à partir des paramètres fournis.
         * @param type Le type de message.
         * @param action L'action du robot associée (peut être null).
         * @param status Le statut de la partie.
         * @param data Les données du message.
         * @return Une chaîne JSON représentant le message.
         */
        public static string BuildMessage(MessageType type,  GameAction? action, GameStatus? status, string data)
        {
            var message =  new Message
            {
                MessageType = type,
                Action = action,
                Status = status,
                Data = data
            };
            return JsonSerializer.Serialize(message, JsonSerializerOption);
           
        }

        /**
         * Convertit une chqîne de caractère au format JSON en un objet Message.
         * @param message La chaîne de caractère représentant le message.
         * @return L'objet Message correspondant.
         * @throws SerializeException Si la désérialisation échoue.
         */
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
