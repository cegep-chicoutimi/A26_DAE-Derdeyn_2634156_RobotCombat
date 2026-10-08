using RobotCombat.Domain.Communication.Transfer;
using RobotCombat.Domain.Game;

namespace RobotCombat.Domain.Commands
{

    /// <summary>
    /// REPLAY Commande pour gérer la demande de rejouer d'un joueur.
    /// </summary>
    public class PlayerReplayHandler(GameController controller, IGameView view) : ICommand
    {
        public void Handle(Message message)
        {
            view.ShowMessage("L'adversaire souhaite rejouer une partie.");
            controller.OnOpponentReplay();
        }
    }
}
