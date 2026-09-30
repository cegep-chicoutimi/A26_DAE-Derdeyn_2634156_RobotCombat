using RobotCombat.Domain.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace RobotCombat.Domain.Communication.Transfer
{
    /**
     * Représente un message échangé entre le client et le serveur.
     * Contient le type de message, l'action du robot associée , les données du message et le statut de la partie.
     */
    public record Message
    {
        public MessageType Type { get; init; }
        public GameAction? Action { get; init; }
        public GameStatus? Status { get; init; }
        public required string Data { get; init; }

    }
}
