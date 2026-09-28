using RobotCombat.Domain.Commands;
using RobotCombat.Domain.Communication;
using RobotCombat.Domain.Communication.Transfer;
using System.Net.Sockets;
using System.Text.Json;

namespace RobotCombat.Domain.Game
{
    /**
     * Contrôleur principal du jeu, gère la logique de jeu et la communication entre les joueurs.
     */
    public class GameController(bool isHost, Config config, IGameView view, CommandMenu menu, ISocket socket)
    {
        public static readonly JsonSerializerOptions RobotConfigJsonOptions = new() { PropertyNameCaseInsensitive = true };
        public bool IsHost { get; } = isHost;
        private readonly Config config = config;
        private readonly IGameView view = view;
        private readonly CommandMenu menu = menu;
        private readonly ISocket socket = socket;

        private readonly object sync = new();
        private RobotConfig? _hostRobotConfig;
        private RobotConfig? _playerRobotConfig;
        public Game? CurrentGame { get; private set; }

        public bool IsLocalTurn => CurrentGame != null && CurrentGame.CurrentRobot.IsHost == IsHost;

        public GameStatus GetGameStatus() => CurrentGame?.Status ?? GameStatus.WAITING_FOR_PLAYER;

        /**
         * Démarre la partie de jeu, envoie un message de bienvenue si l'utilisateur est l'hôte et soumet la configuration locale.
         */
        public async Task StartGame()
        {
            // démarre le socket et envoyer un msg bienvenue
            if (!socket.IsConnected())
            {
                await socket.Start();
                if (IsHost)
                {
                    await Send(MessageType.WELCOME, null, "Hôte");
                }
            }

            if (IsHost)
            {
                await AskLocalConfig();
            }
        }

        /**
         * Déconnecte le socket actuel de la partie.
         */
        public void Disconnect() => socket.Exit();

        /**
         * Soumet la configuration locale du joueur et l'envoie à l'adversaire.
         */
        public async Task AskLocalConfig()
        {
            RobotConfig localConfig = view.AskPlayerConfig();

            if (IsHost)
            {
                ConfigureHost(localConfig);
            }
            else
            {
                ConfigurePlayer(localConfig);
            }

            await Send(MessageType.ROBOT, null, JsonSerializer.Serialize(localConfig, RobotConfigJsonOptions));
        }
        /**
         * Configure le robot de l'hôte et crée une nouvelle partie si les deux configurations sont disponibles.
         */
        public void ConfigureHost(RobotConfig hostConfig)
        {
            lock (sync)
            {
                ResetIfEnded();
                _hostRobotConfig = hostConfig;
                CreateGame();
            }
        }

        /**
         * Configure le robot du joueur et crée une nouvelle partie si les deux configurations sont disponibles.
         */
        public void ConfigurePlayer(RobotConfig playerConfig)
        {
            lock (sync)
            {
                ResetIfEnded();
                _playerRobotConfig = playerConfig;
                CreateGame();
            }
        }

        /**
         * Si la partie précédente est terminée, on repart de zéro.
         * Appelée aussi à la réception d'une config : si l'adversaire a répondu « rejouer »
         * plus vite que nous, sa nouvelle config est gardée au lieu d'être effacée.
         */
        private void ResetIfEnded()
        {
            if (CurrentGame != null && CurrentGame.Status == GameStatus.END_GAME)
            {
                CurrentGame = null;
                _hostRobotConfig = null;
                _playerRobotConfig = null;
            }
        }

        /**
         * Crée une nouvelle partie si les deux configurations sont disponibles.
         */
        public void CreateGame()
        {
            if (CurrentGame == null && _hostRobotConfig != null && _playerRobotConfig != null)
            {
                var hostRobot = new Robot(true, _hostRobotConfig, config);
                var playerRobot = new Robot(false, _playerRobotConfig, config);

                CurrentGame = new Game(config, hostRobot, playerRobot);
                CurrentGame.StartGame();
                DisplayFight();
                var hostRobotEnergy = CurrentGame.robots[0].GetStats(StatsType.ENERGY);
                var playerRobotEnergy = CurrentGame.robots[1].GetStats(StatsType.ENERGY);
                var hostHp = CurrentGame.robots[0].GetStats(StatsType.HP);
                var playerHp = CurrentGame.robots[1].GetStats(StatsType.HP);
                Send(MessageType.START, null, $"{hostRobotEnergy},{playerRobotEnergy},{hostHp},{playerHp}");
            }
        }

        /**
         * Nouvelle partie sur la même connexion : remise à zéro puis
         * nouvelle configuration demandée et envoyée, pour l'hôte ET pour le client.
         */
        public async Task Replay()
        {
            lock (sync)
            {
                ResetIfEnded();
            }
            await AskLocalConfig();
        }

        /**
         * Exécute l'action du joueur local si c'est son tour, applique l'action au jeu et envoie le résultat à l'adversaire.
         */
        public async Task ExecuteActionAsync(GameAction action)
        {
            if (!IsLocalTurn)
            {
                view.ShowMessage("Ce n'est pas votre tour.");
                return;
            }

            if (CurrentGame == null)
            {
                view.ShowMessage("Le jeu n'a pas encore commencé.");
                return;
            }

            int result = CurrentGame.ApplyAction(action);

            while (result == -1)
            {
                view.ShowMessage("Énergie insuffisante, choissez une autre action.");
                action = view.AskPlayerAction();
                result = CurrentGame.ApplyAction(action);
            }

            await Send(MessageType.ACTION, action, result.ToString());
            DisplayFight();
            view.ShowMessage($"Votre action > {GameActionCompanion.ResultOfAction(action, result)}");
            ShowWinnerIfEnded();
        }

        /**
         * Affiche le gagnant si la partie est terminée.
         */
        public void ShowWinnerIfEnded()
        {
            if (CurrentGame != null && CurrentGame.Status == GameStatus.END_GAME)
            {
                if(CurrentGame.GetWinner() != null)
                {
                    view.ShowWinner(CurrentGame.GetWinner());
                }
             
            }
        }
        /**
         * Affiche les statistiques du combat entre les deux robots.
         */
        public void DisplayFight()
        {
            if (CurrentGame != null)
            {
                var local = IsHost ? CurrentGame.robots[0] : CurrentGame.robots[1];
                var remote = IsHost ? CurrentGame.robots[1] : CurrentGame.robots[0];
                view.DisplayFight(local, remote);
            }
        }

        /**
         * Envoie un message à l'adversaire
         * 
         * 
         */
        public Task Send(MessageType type, GameAction? action, string data) =>
            socket.Send(MessageHelper.BuildMessage(type, action, GetGameStatus(), data));

        public async Task Listen()
        {
            string? raw;
            while ((raw = await socket.Receive()) != null)
            {
                OnMessageReceived(MessageHelper.ParseMessage(raw));
            }
        }

        private void OnMessageReceived(Message message) => menu.Execute(message);
    }
}
