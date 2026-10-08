using RobotCombat.Domain.Communication.Transfer;
using RobotCombat.Domain.Game;

namespace RobotCombat.Domain.Commands
{
    /// <summary>
    /// RESULT Commande pour gérer les résultats des actions du joueur.
    /// </summary>
    public class PlayerResultHandler(GameController controller, IGameView view) : ICommand
    {
        public void Handle(Message message)
        {
            if (controller.IsHost)
            {
                return; // le serveur ne doit jamais recevoir ces messages
            }

            switch (message.Type)
            {
                case MessageType.GAME_START:
                    controller.ApplyServerStart(message.Data);
                    break;
                case MessageType.TURN:
                    controller.ApplyServerTurn(message.Data);
                    break;
                case MessageType.ERROR:
                    controller.ApplyServerError(message.Data);
                    break;
                case MessageType.PLAYER_RESULT when message.Action.HasValue:
                    controller.ApplyServerResult(message.Action.Value, message.Data);
                    break;
                case MessageType.GAME_END:
                    controller.ApplyServerEnd(message.Data);
                    break;
                case MessageType.PLAYER_JOIN:
                        view.ShowMessage("Le joueur a rejoint la partie");
                       break;
                default:
                    view.ShowMessage($"Message du serveur invalide : {message.Type}");
                    break;
            }
        }
    }
}
