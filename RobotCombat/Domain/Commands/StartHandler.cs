using RobotCombat.Domain.Communication.Transfer;
using System;
using System.Collections.Generic;
using System.Text;

namespace RobotCombat.Domain.Commands
{
    /// <summary>
    /// START Commande pour gérer le démarrage de la partie.
    /// </summary>
    public class StartHandler(IGameView view) : ICommand
    {
        public void Handle(Message message)
        {
            view.ShowMessage("La partie a commencé");
        }
    }
}
