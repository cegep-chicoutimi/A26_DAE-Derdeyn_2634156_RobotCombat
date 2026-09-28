using RobotCombat.Domain.Communication.Transfer;
using RobotCombat.Domain.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace RobotCombat.Domain.Commands
{
    /**
     * Commande pour gérer le départ d'un joueur.
     */
    public class PlayerLeaveHandler(IGameView view) : ICommand
    {
        public void Handle(Message message)
        {
            view.ShowMessage("L'adversaire a quitté avant la fin de la partie.");
        }
    }
}
