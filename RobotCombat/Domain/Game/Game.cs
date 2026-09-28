using System;
using System.Collections.Generic;
using System.Linq;

namespace RobotCombat.Domain.Game
{
    /**
     * Représente une partie de combat entre deux robots.
     * Gère l'état de la partie, les actions des joueurs et le tour actuel.
     */
    public class Game
    {
        private readonly string id;
        public readonly List<Robot> robots;
        private GameStatus status;
        private int currentPlayer;

        public string Id => id;
        public GameStatus Status => status;
        public Robot CurrentRobot => robots[currentPlayer];
        public Robot OpponentRobot => robots[1 - currentPlayer];

        public Game(Config config, Robot hostRobot, Robot playerRobot)
        {
            id = Guid.NewGuid().ToString();
            robots = new List<Robot> { hostRobot, playerRobot };
            status = GameStatus.WAITING_FOR_PLAYER_CONFIG;
            currentPlayer = 0; // L'hote commence toujours la partie
        }
        /**
         * Démarre la partie
         */
        public void StartGame()
        {
            status = GameStatus.PLAYING;
            currentPlayer = 0;
        }

        /**
         * Applique l'action du joueur actuel sur le robot adverse
          * Met à jour l'état de la partie et change le tour si nécessaire.
          * @param action L'action à appliquer (attaque, défense, recharge).
          * @return Le résultat de l'action (dégâts infligés ou 0 si aucune action).
          */
        public int ApplyAction(GameAction action)
        {
            var attacker = CurrentRobot;
            var defender = OpponentRobot;
            int result = 0;

            switch (action)
            {
                case GameAction.ATTACK:
                    result = defender.ReceiveDamage(attacker.Attack());
                    break;
                case GameAction.ATTACK_PUISSANCE:
                    if (!attacker.CanAttackWithPower())
                    {
                        return -1; // énergie insuffisante
                    }
                    result = defender.ReceiveDamage(attacker.AttackWithPower());
                    break;
                case GameAction.DEFENSE:
                    attacker.Defend();
                    break;
                case GameAction.RECHARGE:
                    attacker.Recharge();
                    break;
            }

            if (CheckGameEnded())
            {
                status = GameStatus.END_GAME;
            }
            else
            {
                currentPlayer = 1 - currentPlayer;
            }

            return result;
        }

        /**
         * (Client) Recopie l'état calculé par le serveur, sans aucun calcul de combat.
         */
        public void CopyState(int hpHost, int hpClient, int energyHost, int energyClient)
        {
            robots[0].ForceStat(hpHost, energyHost);
            robots[1].ForceStat(hpClient, energyClient);

            if (CheckGameEnded())
            {
                status = GameStatus.END_GAME;
            }
        }

        /**
         * Vérifie si la partie est terminée (si un des robots est détruit).
         * @return true si la partie est terminée, false sinon.
         */
        public bool CheckGameEnded()
        {
            return robots.Any(r => !r.IsAlive());
        }

        /**
         * Retourne le gagnant de la partie, si la partie est terminée.
         * @return Le robot gagnant, ou null si la partie n'est pas terminée.
         */
        public Robot? GetWinner()
        {
            return CheckGameEnded() ? robots.FirstOrDefault(r => r.IsAlive()) : null;
        }
    }
}
