using RobotCombat.Domain.Communication.Transfer;
using RobotCombat.Domain.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace RobotCombat.Domain.Commands
{
    public class RobotOkHandler(IGameView view) : ICommand
    {
        public void Handle(Message message)
        {
            view.ShowMessage("Configuration verrouillée.");
        }
    }
}
