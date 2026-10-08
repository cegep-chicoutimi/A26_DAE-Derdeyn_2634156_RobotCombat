using RobotCombat.Domain.Communication.Transfer;
using RobotCombat.Domain.Game;

namespace RobotCombat.Domain.Commands
{
    /// <summary>
    /// WELCOME Commande pour gérer la connexion d'un joueur.
    /// </summary>
    public class PlayerJoinHandler(GameController controller, IGameView view) : ICommand
    {
        
        public void Handle(Message message)
        {
            
            view.ShowMessage($"Connecté à {message.Data} !\n");
            _ = controller.AskLocalConfig();
        }
    }
}
