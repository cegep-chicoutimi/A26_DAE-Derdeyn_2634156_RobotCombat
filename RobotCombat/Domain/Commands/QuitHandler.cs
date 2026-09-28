using RobotCombat.Domain.Communication.Transfer;
using RobotCombat.Domain.Game;

namespace RobotCombat.Domain.Commands
{
    /**
     * QUIT Commande pour gérer la déconnexion d'un joueur.
     */
    public class QuitHandler(GameController controller, IGameView view) : ICommand
    {
        public void Handle(Message message)
        {
            view.ShowMessage("L'adversaire a quitté la partie.");
            controller.Disconnect();
        }
    }
}
