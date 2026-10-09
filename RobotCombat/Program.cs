using RobotCombat.Domain;
using RobotCombat.Domain.Commands;
using RobotCombat.Domain.Communication;
using RobotCombat.Domain.Communication.Transfer;
using RobotCombat.Domain.Game;
using RobotCombat.Views;
using Serilog;
using System.Net.Sockets;

var ipAddress = "127.0.0.1";
try
{
    using var ipSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    ipSocket.Connect("8.8.8.8", 8888);
    ipAddress = ((System.Net.IPEndPoint)ipSocket.LocalEndPoint).Address.ToString();
}
catch (SocketException)
{
    Console.WriteLine("Impossible de déterminer l'adresse IP locale. Veuillez vérifier votre connexion réseau. Utilisation de l'ip locale "+ipAddress);
    return;
}

var gameConfig = new Config(IpAddress: ipAddress);
var consoleGameView = new ConsoleView(gameConfig);
var randomize = new Randomize(gameConfig);
var isHost = consoleGameView.AskPlayerType().Equals("HOST");
var startTime = DateTime.Now.ToString("yyMMdd_HHmm");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File(
        path: $"logs/{startTime}_{(isHost ? "host" : "client")}-.log",
        outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {SourceContext} | {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

// Création socket en fonction du type de joueur
ISocket socket;
if (isHost)
{
    var hostInfo = consoleGameView.AskHostPortInformation();
    socket = new SocketServer(hostInfo, gameConfig.IpAddress);
}
else
{
    var hostInfo = consoleGameView.AskPlayerHostInformations();
    socket = new SocketClient(hostInfo[0], int.Parse(hostInfo[1]));
}


bool keepRunning = true;
// Boucle principale permettant à l'hôte de relancer des parties en boucle
do
{
    var commandMenu = new CommandMenu();
    var gameController = new GameController(isHost, gameConfig, consoleGameView, commandMenu, socket, randomize);

    commandMenu.AddHandler(MessageType.WELCOME, new PlayerJoinHandler(gameController, consoleGameView));
    commandMenu.AddHandler(MessageType.ROBOT_CONFIG, new RobotReadyHandler(gameController, consoleGameView));
    commandMenu.AddHandler(MessageType.ROBOT_CONFIG_OK, new RobotOkHandler(consoleGameView));
    commandMenu.AddHandler(MessageType.PLAYER_ACTION, new PlayerActionHandler(gameController, consoleGameView));
    var serverMessageHandler = new PlayerResultHandler(gameController, consoleGameView);
    commandMenu.AddHandler(MessageType.PLAYER_JOIN, serverMessageHandler);
    commandMenu.AddHandler(MessageType.GAME_START, serverMessageHandler);
    commandMenu.AddHandler(MessageType.TURN, serverMessageHandler);
    commandMenu.AddHandler(MessageType.PLAYER_RESULT, serverMessageHandler);
    commandMenu.AddHandler(MessageType.GAME_END, serverMessageHandler);
    commandMenu.AddHandler(MessageType.ERROR, serverMessageHandler);
    commandMenu.AddHandler(MessageType.PLAYER_REPLAY, new PlayerReplayHandler(gameController, consoleGameView));
    commandMenu.AddHandler(MessageType.QUIT, new QuitHandler(gameController, consoleGameView));

    
    try
    {
        consoleGameView.ShowMessage(isHost ? "Votre addrese IP: " + gameConfig.IpAddress + "\nEn attente d'un adversaire..." : "Connexion à l'hôte...");
        if (!await gameController.StartGame())
        {
            continue; // client refusé (SERVEUR_BUSY)
        }
        var listenTask = gameController.Listen();


        // En attente du lancement de la partie
        while (gameController.GetGameStatus() != GameStatus.PLAYING && !listenTask.IsCompleted)
        {
            await Task.Delay(200);
        }

        // Si la tâche d'écoute a échoué, afficher un message d'erreur
        if (listenTask.IsFaulted)
        {
            Log.Error(listenTask.Exception, "Erreur d'écoute");
            consoleGameView.ShowMessage("Connexion perdue avec l'hôte.");
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
                        await gameController.Send(MessageType.PLAYER_REPLAY, null, "OK");
                    }
                }
                else
                { // joueur externe
                    // demander au joueur s'il souhaite rejouer
                    replay = consoleGameView.AskPlayerReplay();

                    if (listenTask.IsCompleted)
                    { // erreur l hôte s'est déconnecté
                        consoleGameView.ShowMessage("L'hôte a quitté, impossible de rejouer.");
                        replay = false;
                    }
                    else if (replay)
                    {
                        await gameController.Send(MessageType.PLAYER_REPLAY, null, "");
                        
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
        Log.Error(ex, "Erreur inattendue");
        consoleGameView.ShowMessage("Une erreur est survenue.");
    }
    finally
    {
        if (!isHost)
        {
            socket.Exit();
            keepRunning = false;
        }
    }

} while (isHost && keepRunning);

if (isHost)
{
    socket.Exit(); 
} else
{
    consoleGameView.ShowMessage("Appuyez sur une touche pour quitter...");
    Console.ReadKey();
}

Log.CloseAndFlush();