using RobotCombat.Domain.Communication.Transfer;
using RobotCombat.Domain.Game;

namespace RobotCombat.Domain.Commands
{
    /**
     * ACTION (reçu par le client) : l'adversaire a joué un coup.
     * Le client applique l'action reçue à son robot et affiche le résultat.
     */
    public class PlayerActionHandler(GameController controller, IGameView view) : ICommand
    {
        public void Handle(Message message)
        {
            var game = controller.CurrentGame;

            if (game == null || !message.Action.HasValue || game.Status != GameStatus.PLAYING)
            {
                view.ShowMessage("Action reçue invalide ou hors partie.");
            }
            else if (controller.IsLocalTurn)
            {
                // Action de l'adversaire reçue hors de son tour : ignorée
                view.ShowMessage("Action de l'adversaire reçue hors de son tour : ignorée.");
            }
            else
            {
                int result = game.ApplyAction(message.Action.Value);
                controller.DisplayFight();
                view.ShowMessage($"Action de l'adversaire > {GameActionCompanion.ResultOfAction(message.Action.Value, result)}");
                controller.ShowWinnerIfEnded();
            }
        }
    }
}
