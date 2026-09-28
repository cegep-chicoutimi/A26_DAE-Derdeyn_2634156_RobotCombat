using RobotCombat.Domain.Communication.Transfer;
using RobotCombat.Domain.Game;
using System.Text.Json;

namespace RobotCombat.Domain.Commands
{
    /**
     * Commande pour gérer la configuration du robot adverse.
     */
    public class RobotReadyHandler(GameController controller, IGameView view) : ICommand
    {
        public void Handle(Message message)
        {
            RobotConfig? opponent = null;
            try
            {
                opponent = JsonSerializer.Deserialize<RobotConfig>(message.Data, GameController.RobotConfigJsonOptions);
            }
            catch (JsonException)
            { }

            if (opponent == null || !opponent.IsValid())
            {
                view.ShowMessage("Configuration adverse invalide.");
            }
            else if (controller.IsHost)
            {
                controller.ConfigurePlayer(opponent);   // l'hôte reçoit la config du client
            }
            else
            {
                controller.ConfigureHost(opponent);     // le client reçoit la config de l'hôte
            }
        }
    }
}
