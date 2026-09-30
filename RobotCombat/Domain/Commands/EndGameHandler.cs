
using RobotCombat.Domain.Communication.Transfer;
using RobotCombat.Domain.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace RobotCombat.Domain.Commands
{
 
    public class EndGameHandler(GameController controller, IGameView view) : ICommand
    {
        public void Handle(Message message)
        {
          
        }
    }
}
