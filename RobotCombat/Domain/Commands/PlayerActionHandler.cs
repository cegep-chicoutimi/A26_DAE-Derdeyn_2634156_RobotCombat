using RobotCombat.Domain.Communication.Transfer;
using RobotCombat.Domain.Game;

namespace RobotCombat.Domain.Commands
{
    /// <summary>
    /// ACTION : le client a choisi une action.
    /// </summary>
    public class PlayerActionHandler(GameController controller, IGameView view) : ICommand
    {
        public void Handle(Message message)
        {
            if (!controller.IsHost)
            {
                view.ShowMessage("Message ACTION ignoré : seul le serveur résout les actions.");
            }
            else if (!message.Action.HasValue)
            {
                _ = controller.Send(MessageType.ERROR, null, "INVALID_ACTION");
            }
            else
            {
                _ = controller.HandleClientActionAsync(message.Action.Value);
            }
        }
    }
}
