using RobotCombat.Domain.Commands;
using RobotCombat.Domain.Communication;
using RobotCombat.Domain.Communication.Transfer;
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

        /**
         * Vrai quand c'est au tour du joueur local de jouer, faux sinon
         */
        private volatile bool localTurn;

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
            string json = JsonSerializer.Serialize(localConfig, RobotConfigJsonOptions);

            if (IsHost)
            {
                // envoie de la config hôte avant
                await Send(MessageType.ROBOT, null, json);
                ConfigureHost(localConfig);
            }
            else
            {
                ConfigurePlayer(localConfig);
                await Send(MessageType.ROBOT, null, json);
            }
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
                var hostRobot = new Robot(true, _hostRobotConfig, config);
                var playerRobot = new Robot(false, _playerRobotConfig, config);

                CurrentGame = new Game(config, hostRobot, playerRobot);
                localTurn = false;

                if (IsHost)
                {
                    CurrentGame.StartGame();
                    DisplayFight();
                    _ = StartHostGameAsync();
                }
            }
        }

        /**
         * Envoie un message START
         */
        private async Task StartHostGameAsync()
        {
            await Send(MessageType.START, null, BuildStateData());
            await SendTurnAsync();
        }

        /**
         * Nouvelle partie sur la même connexion
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
                await Send(MessageType.ACTION, action, "");
                view.ShowMessage("Action envoyée, en attente du serveur...");
                return;
            }

            while (!await ResolveActionAsync(action))
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

            if (!await ResolveActionAsync(action))
            {
                await Send(MessageType.ERROR, action, "INVALID_ACTION");
            }
        }

        /**
         * Résolution d'une action, exécutée par le serveur pour les 2 joueurs.
         */
        private async Task<bool> ResolveActionAsync(GameAction action)
        {
            Game game = CurrentGame!;
            bool hostActed = game.CurrentRobot.IsHost;

            int damage = game.ApplyAction(action);
            if (damage == -1)
            {
                return false;
            }

            await Send(MessageType.RESULT, action, $"{TurnName(hostActed)};{damage};{BuildStateData()}");
            ShowActionResult(hostActed, action, damage);

            if (game.Status == GameStatus.PLAYING)
            {
                await SendTurnAsync();
            }
            return true;
        }

        /**
         * Annonce à qui est le tour : TURN;HOTE ou TURN;CLIENT.
         */
        private async Task SendTurnAsync()
        {
            bool hostTurn = CurrentGame!.CurrentRobot.IsHost;
            await Send(MessageType.TURN, null, TurnName(hostTurn));

            if (hostTurn)
            {
                localTurn = true;   // débloque la saisie de l'hôte
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
            if (CurrentGame == null)
            {
                view.ShowMessage("START reçu alors que la partie n'est pas prête.");
                return;
            }

            int[] state = ParseState(data.Split(';'), 0);
            CurrentGame.StartGame();
            CurrentGame.CopyState(state[0], state[1], state[2], state[3]);
            DisplayFight();
        }

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
            int damage = int.Parse(parts[1]);
            int[] state = ParseState(parts, 2);

            CurrentGame.CopyState(state[0], state[1], state[2], state[3]);
            ShowActionResult(hostActed, action, damage);
        }

        /**
         * Affiche le combat et le résultat d'une action, puis le gagnant si la partie est finie.
         */
        private void ShowActionResult(bool hostActed, GameAction action, int damage)
        {
            bool isMine = hostActed == IsHost;
            DisplayFight();
            view.ShowMessage($"{(isMine ? "Votre action" : "Action de l'adversaire")} > {action.ResultOfAction(damage)}");
            ShowWinnerIfEnded();
        }

        /**
         * État de la partie au format {pvHote};{pvClient};{energieHote};{energieClient}.
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

        /**
         * Affiche le gagnant si la partie est terminée.
         */
        public void ShowWinnerIfEnded()
        {
            if (CurrentGame != null && CurrentGame.Status == GameStatus.END_GAME)
            {
                Robot? winner = CurrentGame.GetWinner();
                if (winner != null)
                {
                    view.ShowWinner(winner);
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
