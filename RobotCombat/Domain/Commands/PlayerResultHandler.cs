using RobotCombat.Domain.Communication.Transfer;
using RobotCombat.Domain.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace RobotCombat.Domain.Commands
{
    /**
     * Commande pour gérer le résultat d'une action du joueur.
     */
    public class PlayerResultHandler(IGameView view) : ICommand
    {
        public void Handle(Message message)
        {
            view.ShowMessage($"Confirmation de l'adversaire : {message.Data} dégâts.");
        }
    }
}
