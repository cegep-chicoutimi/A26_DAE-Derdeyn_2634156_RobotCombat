using RobotCombat.Domain.Communication.Transfer;
using RobotCombat.Domain.Game;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace RobotCombat.Domain.Commands
{
    /// <summary>
    /// Classe qui gère l'association entre les types de messages et leurs commandes correspondantes.
    /// </summary>
    public class CommandMenu
    {
        private readonly Dictionary<MessageType, ICommand> handlers = [];

        /// <summary>
        /// Ajoute un gestionnaire pour un type de message spécifique.
        /// </summary>
        /// <param name="messageType">Le type de message à gérer.</param>
        /// <param name="command">La commande à exécuter pour ce type de message.</param>
        public void AddHandler(MessageType messageType, ICommand command)
        {
            handlers[messageType] = command;
        }

        /// <summary>
        /// Exécute la commande associée au type de message reçu.
        /// </summary>
        /// <param name="message">Le message à traiter.</param>
        /// <exception cref="Exception">Si aucun gestionnaire n'est trouvé pour le type de message.</exception>
        public void Execute(Message message)
        {
            if (handlers.TryGetValue(message.Type, out var handler))
            {
                handler.Handle(message);
            }
            else
            {
                throw new Exception($"No handler found for message type: {message.Type}");
            }
        }
    }
}
