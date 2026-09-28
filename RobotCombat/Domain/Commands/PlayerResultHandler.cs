using RobotCombat.Domain.Communication.Transfer;
using RobotCombat.Domain.Game;

namespace RobotCombat.Domain.Commands
{
    /**
     * Messages envoyés par le SERVEUR au client pendant le combat (START, TURN, RESULT, ERROR).
     */
    public class PlayerResultHandler(GameController controller, IGameView view) : ICommand
    {
        public void Handle(Message message)
        {
            if (controller.IsHost)
            {
                return; // le serveur ne doit jamais recevoir ces messages
            }

            switch (message.MessageType)
            {
                case MessageType.START:
                    controller.ApplyServerStart(message.Data);
                    break;
                case MessageType.TURN:
                    controller.ApplyServerTurn(message.Data);
                    break;
                case MessageType.ERROR:
                    controller.ApplyServerError(message.Data);
                    break;
                case MessageType.RESULT when message.Action.HasValue:
                    controller.ApplyServerResult(message.Action.Value, message.Data);
                    break;
                default:
                    view.ShowMessage($"Message du serveur invalide : {message.MessageType}");
                    break;
            }
        }
    }
}
