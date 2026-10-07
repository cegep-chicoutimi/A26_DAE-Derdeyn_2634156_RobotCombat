using RobotCombat.Domain.Commands;
using RobotCombat.Domain.Communication;
using RobotCombat.Domain.Communication.Transfer;

namespace RobotCombat.Domain.Game
{
    /**
     * Contrôleur principal du jeu, gère la logique de jeu et la communication entre les joueurs.
     */
    public class GameController(bool isHost, Config config, IGameView view, CommandMenu menu, ISocket socket, Randomize randomize)
    {
        public bool IsHost { get; } = isHost;
        private readonly Config config = config;
        private readonly IGameView view = view;
        private readonly CommandMenu menu = menu;
        private readonly ISocket socket = socket;

        private RobotConfig? _hostRobotConfig;
        private RobotConfig? _playerRobotConfig;

        private bool localTurn;

        public Game? CurrentGame { get; private set; }

        public bool IsLocalTurn => localTurn && GetGameStatus() == GameStatus.PLAYING;

        public GameStatus GetGameStatus() => CurrentGame?.Status ?? GameStatus.WAITING_FOR_PLAYER;

        /**
         * Démarre la partie de jeu, envoie un message de bienvenue si l'utilisateur est l'hôte et soumet la configuration locale.
         */
        public async Task StartGame()
        {
            if (!socket.IsConnected())
            {
                await socket.Start();
                if (IsHost)
                {
                    await Send(MessageType.WELCOME, null, "Hôte");
                } else
                {
                    await Send(MessageType.PLAYER_JOIN, null, "");
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

            while (!localConfig.IsValid())
            {
                view.ShowMessage("Configuration invalide, veuillez réassayer.");
                localConfig = view.AskPlayerConfig();
            }

            if (IsHost)
            {
                ConfigureHost(localConfig);
                return;
            }


            _playerRobotConfig = localConfig;
            await Send(MessageType.ROBOT_CONFIG, null, $"{localConfig.HpPoints};{localConfig.ArmorPoints};{localConfig.DamagePoints}");
        }

        /**
         * Configurer le robot du joueur et créer une nouvelle partie si les deux configurations sont disponibles.
         */
        public async Task HandleClientConfig(RobotConfig? robotConfig)
        {

            if (robotConfig == null || !robotConfig.IsValid())
            {
                await Send(MessageType.ERROR, null, "INVALID_CONFIG");
                return;
            }

            await Send(MessageType.ROBOT_CONFIG_OK, null, "OK");
            ConfigurePlayer(robotConfig);
        }

        /**
         * Configure le robot de l'hôte et crée une nouvelle partie si les deux configurations sont disponibles.
         */
        public void ConfigureHost(RobotConfig hostConfig)
        {

            ResetIfEnded();
            _hostRobotConfig = hostConfig;
            CreateGame();

        }

        /**
         * Configure le robot du joueur et crée une nouvelle partie si les deux configurations sont disponibles.
         */
        public void ConfigurePlayer(RobotConfig playerConfig)
        {
            ResetIfEnded();
            _playerRobotConfig = playerConfig;
            CreateGame();

        }

        /**
         * Réinitialise la partie si elle est terminée
         */
        private void ResetIfEnded()
        {
            if (CurrentGame != null && CurrentGame.Status == GameStatus.END_GAME)
            {
                CurrentGame = null;
                _hostRobotConfig = null;
                _playerRobotConfig = null;
                localTurn = false;
            }
        }

        /**
         * Crée une nouvelle partie si les deux configurations sont disponibles
         */
        public void CreateGame()
        {
            if (CurrentGame == null && _hostRobotConfig != null && _playerRobotConfig != null)
            {
                var hostRobot = new Robot(true, _hostRobotConfig, config, randomize);
                var playerRobot = new Robot(false, _playerRobotConfig, config, randomize);

                CurrentGame = new Game(config, hostRobot, playerRobot);
                localTurn = false;

                // Seul le serveur crée la partie (le client ne connaît pas la config de l'hôte)
                CurrentGame.StartGame();
                DisplayFight();
                _ = StartHostGame();
            }
        }

        /**
         * Envoie un message START
         */
        private async Task StartHostGame()
        {
            await Send(MessageType.GAME_START, null, BuildStateData());
            await SendTurn();
        }

        /**
         * Nouvelle partie sur la même connexion
         */
        public async Task Replay()
        {
            OpponentWantsReplay = false;

            ResetIfEnded();

            await AskLocalConfig();
        }

        public bool OpponentWantsReplay { get; private set; }

        /**
         * L'adversaire a accepté de rejouer
         */
        public void OnOpponentReplay() => OpponentWantsReplay = true;

        /**
         * Action choisie par le joueur LOCAL
         */
        public async Task ExecuteActionAsync(GameAction action)
        {
            if (CurrentGame == null)
            {
                view.ShowMessage("Le jeu n'a pas encore commencé.");
                return;
            }

            if (!IsLocalTurn)
            {
                view.ShowMessage("Ce n'est pas votre tour.");
                return;
            }

            localTurn = false;

            if (!IsHost)
            {
                await Send(MessageType.PLAYER_ACTION, action, "");
                // view.ShowMessage("Action envoyée, en attente du serveur...");
                return;
            }

            while (!await ResolveAction(action))
            {
                view.ShowMessage("Énergie insuffisante : choisissez une autre action.");
                action = view.AskPlayerAction();
            }
        }

        /**
         *  Action reçue du client
         */
        public async Task HandleClientActionAsync(GameAction action)
        {
            if (!IsHost || CurrentGame == null || CurrentGame.Status != GameStatus.PLAYING || CurrentGame.CurrentRobot.IsHost)
            {
                await Send(MessageType.ERROR, action, "NOT_YOUR_TURN");
                return;
            }

            if (!await ResolveAction(action))
            {
                await Send(MessageType.ERROR, action, "INVALID_ACTION");
            }
        }

        /**
         * Résolution d'une action, exécutée par le serveur pour les 2 joueurs.
         */
        private async Task<bool> ResolveAction(GameAction action)
        {
            Game game = CurrentGame!;
            bool hostActed = game.CurrentRobot.IsHost;

            int value = game.ApplyAction(action, out bool actionCompleted);
            if (value == -1)
            {
                return false;
            }

            // RESULT;{HOTE|CLIENT};{action};{actionCompleted};{degats};{pvHote};{pvClient};{energieHote};{energieClient}
            await Send(MessageType.PLAYER_RESULT, action, $"{TurnName(hostActed)};{action};{actionCompleted};{value};{BuildStateData()}");
            ShowActionResult(hostActed, action, actionCompleted, value);

            if (game.Status == GameStatus.PLAYING)
            {
                await SendTurn();
            }
            return true;
        }

        /**
         * Annonce à qui est le tour : TURN;HOTE ou TURN;CLIENT.
         */
        private async Task SendTurn()
        {
            bool hostTurn = CurrentGame!.CurrentRobot.IsHost;
            await Send(MessageType.TURN, null, TurnName(hostTurn));

            if (hostTurn)
            {
                localTurn = true;
            }
            else
            {
                view.ShowMessage("Tour de l'adversaire…");
            }
        }

        /**
         * Démarre la partie avec l'état envoyé par le serveur.
         */
        public void ApplyServerStart(string data)
        {
            var hostRobot = new Robot(true, new RobotConfig(), config, randomize);
            var playerRobot = new Robot(false, _playerRobotConfig ?? new RobotConfig(), config, randomize);
            CurrentGame = new Game(config, hostRobot, playerRobot);

            int[] state = ParseState(data.Split(';'), 0);
            CurrentGame.StartGame();
            CurrentGame.CopyState(state[0], state[1], state[2], state[3]);
            DisplayFight();
        }

        /**
         * Annonce à qui est le tour de jouer
         */
        public void ApplyServerTurn(string data)
        {
            if (data == TurnName(false))
            {
                localTurn = true;
            }
            else
            {
                view.ShowMessage("Tour de l'adversaire…");
            }
        }

        /**
         * Action refusée par le serveur, le tour n'est pas consommé.
         */
        public void ApplyServerError(string data)
        {
            if (data == "INVALID_ACTION")
            {
                view.ShowMessage("Énergie insuffisante : choisissez une autre action.");
                localTurn = true;
            }
            else if (data == "INVALID_CONFIG")
            {
                view.ShowMessage("Il faut répartir exactement 10 points.");
                _ = AskLocalConfig();
            }
            else
            {
                view.ShowMessage($"Erreur du serveur : {data}");
            }
        }

        /**
         * Recopier les stats du combat envoyées par le serveur et afficher le résultat de l'action.
         */
        public void ApplyServerResult(GameAction action, string data)
        {
            if (CurrentGame == null)
            {
                return;
            }

            string[] parts = data.Split(';');
            bool hostActed = parts[0] == TurnName(true);
            bool actionCompleted = bool.Parse(parts[2]);
            int value = int.Parse(parts[3]);
            int[] state = ParseState(parts, 4);

            CurrentGame.CopyState(state[0], state[1], state[2], state[3]);
            if (action == GameAction.ESCAPE && actionCompleted)
            {
                CurrentGame.EndByEscape();
            }
            ShowActionResult(hostActed, action, actionCompleted, value);
        }

        /**
         * Affiche le combat et le résultat d'une action et le gagnant si la partie est finie.
         */
        private void ShowActionResult(bool hostActed, GameAction action, bool actionCompleted, int value)
        {
            bool isMine = hostActed == IsHost;
            DisplayFight();
            view.ShowMessage($"{(isMine ? "Votre action" : "Action de l'adversaire")} ({action.ToLabel()}) > {action.ResultOfAction(actionCompleted, value)}");
            _ = ShowWinnerIfEnded();
        }

        /**
         * État de la partie
         */
        private string BuildStateData()
        {
            Robot host = CurrentGame!.robots[0];
            Robot client = CurrentGame.robots[1];
            return $"{host.GetStats(StatsType.HP)};{client.GetStats(StatsType.HP)};" +
                   $"{host.GetStats(StatsType.ENERGY)};{client.GetStats(StatsType.ENERGY)}";
        }

        private static int[] ParseState(string[] parts, int offset) =>
            [int.Parse(parts[offset]), int.Parse(parts[offset + 1]), int.Parse(parts[offset + 2]), int.Parse(parts[offset + 3])];

        private static string TurnName(bool host) => host ? "HOTE" : "CLIENT";

        private const string NO_WINNER = "AUCUN";

        /**
         * Affiche le gagnant si la partie est terminée.
         */
        public async Task ShowWinnerIfEnded()
        {
            if (CurrentGame != null && CurrentGame.Status == GameStatus.END_GAME)
            {
                var hostHp = CurrentGame.robots[0].GetStats(StatsType.HP);
                var clientHP = CurrentGame.robots[1].GetStats(StatsType.HP);
                Robot? winner = CurrentGame.GetWinner();

                if (IsHost)
                {
                    string winnerName = winner == null ? NO_WINNER : TurnName(winner.IsHost);
                    await Send(MessageType.GAME_END, null, string.Join(';', winnerName, hostHp, clientHP));
                }
                view.ShowWinner(winner);
            }
        }

        public void ApplyServerEnd(string data)
        {
            if (CurrentGame == null)
            {
                return;
            }

            string[] parts = data.Split(';');
            int hostHp = int.Parse(parts[1]);
            int clientHp = int.Parse(parts[2]);

            bool alreadyEnded = CurrentGame.Status == GameStatus.END_GAME;

            int hostEnergy = int.Parse(CurrentGame.robots[0].GetStats(StatsType.ENERGY));
            int clientEnergy = int.Parse(CurrentGame.robots[1].GetStats(StatsType.ENERGY));
            CurrentGame.CopyState(hostHp, clientHp, hostEnergy, clientEnergy);
            if (parts[0] == NO_WINNER)
            {
                CurrentGame.EndByEscape();
            }

            if (!alreadyEnded)
            {
                DisplayFight();
                view.ShowWinner(CurrentGame.GetWinner());
            }
        }

        /**
         * Affiche les statistiques du combat entre les deux robots.
         */
        public void DisplayFight()
        {
            if (CurrentGame != null)
            {
                var local = IsHost ? CurrentGame.robots.First() : CurrentGame.robots.Last();
                var remote = IsHost ? CurrentGame.robots.Last() : CurrentGame.robots.First();
                view.DisplayFight(local, remote);
            }
        }

        /**
         * Envoie un message à l'adversaire
         */
        public Task Send(MessageType type, GameAction? action, string data) =>
            socket.Send(MessageHelper.BuildMessage(type, action, GetGameStatus(), data));

        /**
         * Écoute les messages entrants du socket et les traite.
         */
        public async Task Listen()
        {
            string? raw;
            while ((raw = await socket.Receive()) != null)
            {
                OnMessageReceived(MessageHelper.ParseMessage(raw));
            }
        }
        /**
         * Traite un message reçu du socket.
         */
        private void OnMessageReceived(Message message) => menu.Execute(message);
    }
}
