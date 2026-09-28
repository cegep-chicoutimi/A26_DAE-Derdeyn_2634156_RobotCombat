using RobotCombat.Domain;
using RobotCombat.Domain.Commands;
using RobotCombat.Domain.Communication;
using RobotCombat.Domain.Communication.Transfer;
using RobotCombat.Domain.Game;
using RobotCombat.Views;

var gameConfig = new Config();
var consoleGameView = new ConsoleView(gameConfig);

var isHost = consoleGameView.AskPlayerType().Equals("HOST");

// Création socket en fonction du type de joueur
ISocket socket;
if (isHost)
{
    var hostInfo = consoleGameView.AskHostPortInformation();
    socket = new SocketServer(gameConfig.Port);
}
else
{
    var hostInfo = consoleGameView.AskPlayerHostInformations();
    socket = new SocketClient(hostInfo[0], int.Parse(hostInfo[1]));
    await socket.Send($"Join;{hostInfo[2]}");
}

bool keepRunning = true;

do
{
    var commandMenu = new CommandMenu();
    var gameController = new GameController(isHost, gameConfig, consoleGameView, commandMenu, socket);

    commandMenu.AddHandler(MessageType.WELCOME, new PlayerJoinHandler(gameController, consoleGameView));
    commandMenu.AddHandler(MessageType.ROBOT, new RobotReadyHandler(gameController, consoleGameView));
    commandMenu.AddHandler(MessageType.ACTION, new PlayerActionHandler(gameController, consoleGameView));
    var serverMessageHandler = new PlayerResultHandler(gameController, consoleGameView);
    commandMenu.AddHandler(MessageType.START, serverMessageHandler);
    commandMenu.AddHandler(MessageType.TURN, serverMessageHandler);
    commandMenu.AddHandler(MessageType.RESULT, serverMessageHandler);
    commandMenu.AddHandler(MessageType.ERROR, serverMessageHandler);
    commandMenu.AddHandler(MessageType.REPLAY, new PlayerReplayHandler(gameController, consoleGameView));
    commandMenu.AddHandler(MessageType.QUIT, new QuitHandler(gameController, consoleGameView));

    try
    {
        consoleGameView.ShowMessage(isHost ? "En attente d'un adversaire..." : "Connexion à l'hôte...");
        await gameController.StartGame();
        var listenTask = gameController.Listen();


        // En attente du lancement de la partie
        while (gameController.GetGameStatus() != GameStatus.PLAYING && !listenTask.IsCompleted)
        {
            await Task.Delay(200);
        }

        // Si la tâche d'écoute a échoué, afficher le message d'erreur
        if (listenTask.IsFaulted)
        {
            consoleGameView.ShowMessage($"Erreur d'écoute : {listenTask.Exception?.InnerException?.Message}");
        }

        bool replay;
        do
        {
            replay = false;

            // Partie en cours
            while (gameController.GetGameStatus() == GameStatus.PLAYING && !listenTask.IsCompleted)
            {
                if (gameController.IsLocalTurn)
                { // Tour du joueur local
                    var action = consoleGameView.AskPlayerAction();
                    await gameController.ExecuteActionAsync(action);
                }
                else
                { // attente adversaire
                    await Task.Delay(200);
                }
            }

            // Fin de partie
            if (gameController.GetGameStatus() == GameStatus.END_GAME && !listenTask.IsCompleted)
            {
                if (isHost)
                {
                    consoleGameView.ShowMessage("En attente de la revanche du joueur...");
                    while (!gameController.OpponentWantsReplay && !listenTask.IsCompleted)
                    { // Attente décision du joueur pour rejouer une partie
                        await Task.Delay(200);
                    }

                    // Le client a refusé de rejouer ou s'est déconnecté
                    replay = !listenTask.IsCompleted;

                    if (replay)
                    {
                        await gameController.Send(MessageType.REPLAY, null, "");
                    }
                }
                else
                {
                    // demander au joueur s'il souhaite rejouer
                    replay = consoleGameView.AskPlayerReplay();

                    if (listenTask.IsCompleted)
                    { // erreur l hôte s'est déconnecté
                        consoleGameView.ShowMessage("L'hôte a quitté, impossible de rejouer.");
                        replay = false;
                    }
                    else if (replay)
                    {
                        await gameController.Send(MessageType.REPLAY, null, "");
                        
                        // attente réponse de l'hôte
                        while (!gameController.OpponentWantsReplay && !listenTask.IsCompleted)
                        {
                            await Task.Delay(200);
                        }
                        replay = !listenTask.IsCompleted;
                    }
                }

                if (replay)
                {
                    // redémarrer une partie
                    await gameController.Replay();

                    consoleGameView.ShowMessage("En attente de la configuration de l'adversaire...");
                    while (gameController.GetGameStatus() != GameStatus.PLAYING && !listenTask.IsCompleted)
                    {
                        await Task.Delay(200);
                    }

                    if (listenTask.IsCompleted)
                    {
                        consoleGameView.ShowMessage("L'adversaire a quitté. Fin de la partie.");
                        replay = false;
                    }
                }
            }
        } while (replay);

        if (!listenTask.IsCompleted)
        {
            await gameController.Send(MessageType.QUIT, null, "");
        }

        if (isHost && await Task.WhenAny(listenTask, Task.Delay(3000)) != listenTask)
        {
            gameController.Disconnect();
        }
    }
    catch (Exception ex)
    {
        consoleGameView.ShowMessage($"Une erreur est survenue.");
    }
    finally
    {
        if (isHost)
        {
            consoleGameView.ShowMessage("Partie terminée. En attente d'un nouvel adversaire...");
        }
        else
        {
            socket.Exit();
            keepRunning = false;
        }
    }

} while (isHost && keepRunning);

if (isHost)
{
    socket.Exit(); 
}