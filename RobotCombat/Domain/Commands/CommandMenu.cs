using RobotCombat.Domain.Communication.Transfer;
using RobotCombat.Domain.Game;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace RobotCombat.Domain.Commands
{
    /**
     * Classe qui gère l'association entre les types de messages et leurs commandes correspondantes.
     */
    public class CommandMenu
    {
        private readonly Dictionary<MessageType, ICommand> handlers = [];

        /**
         * Ajoute un gestionnaire pour un type de message spécifique.
         * @param messageType Le type de message à gérer.
         * @param command La commande à exécuter pour ce type de message.
         */
        public void AddHandler(MessageType messageType, ICommand command)
        {
            handlers[messageType] = command;
        }

        /**
         * Exécute la commande associée au type de message reçu.
         * @param message Le message à traiter.
         * @throws Exception Si aucun gestionnaire n'est trouvé pour le type de message.
         */
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
