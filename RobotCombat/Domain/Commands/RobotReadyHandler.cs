using RobotCombat.Domain.Communication.Transfer;
using RobotCombat.Domain.Game;

namespace RobotCombat.Domain.Commands
{
    /**
     * ROBOT Commande pour gérer la configuration du robot d'un joueur.
     */
    public class RobotReadyHandler(GameController controller, IGameView view) : ICommand
    {
        public void Handle(Message message)
        {
            if (controller.IsHost)
            {
                var config = ParseRobotConfig(message.Data);
                _ = controller.HandleClientConfig(config);
            }
            else if (message.Data == "OK")
            {
                view.ShowMessage("Configuration verrouillée. En attente de l'adversaire…");
            }
            else
            {
                view.ShowMessage("Message ROBOT inattendu.");
            }
        }
        private static RobotConfig? ParseRobotConfig(string data)
        {
            string[] parts = (data ?? "").Split(';');
            if (parts.Length != 3 || !int.TryParse(parts[0], out int hp) || !int.TryParse(parts[1], out int armor) || !int.TryParse(parts[2], out int damage))
            {
                return null;
            }
            return new RobotConfig { HpPoints = hp, ArmorPoints = armor, DamagePoints = damage };
        }
    }

}
