using RobotCombat.Domain.Communication.Transfer;
using System;
using System.Collections.Generic;
using System.Text;

namespace RobotCombat.Domain.Commands
{
    public class StartHandler(IGameView view) : ICommand
    {
        public void Handle(Message message)
        {
            view.ShowMessage("La partie a commencé");
        }
    }
}
