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
        private bool escaped;

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
            escaped = false;
        }

        /**
         * Applique l'action du joueur actuel (sur lui-même ou sur le robot adverse).
         * Met à jour l'état de la partie et change le tour si nécessaire.
         * @param action L'action à appliquer.
         * @param actionCompleted true si l'action a réussi (false si ratée à cause du hasard).
         * @return Dégâts infligés (attaques) ou PV récupérés (réparation), 0 sinon,
         *         ou -1 si l'action est refusée (énergie insuffisante : le tour n'est pas consommé).
         */
        public int ApplyAction(GameAction action, out bool actionCompleted)
        {
            var attacker = CurrentRobot;
            var defender = OpponentRobot;
            int result = 0;
            actionCompleted = false;

            switch (action)
            {
                case GameAction.ATTACK:
                    result = ResolveAttack(attacker.Attack(), defender, out actionCompleted);
                    break;
                case GameAction.ATTACK_PUISSANCE:
                    if (!attacker.CanAttackWithPower())
                    {
                        return -1; // énergie insuffisante : tour non consommé
                    }
                    result = ResolveAttack(attacker.AttackWithPower(), defender, out actionCompleted);
                    break;
                case GameAction.DEFENSE:
                    actionCompleted = attacker.Defend();
                    break;
                case GameAction.RECHARGE:
                    attacker.Recharge();
                    actionCompleted = true;
                    break;
                case GameAction.REPAIR:
                    int repaired = attacker.Repair();
                    actionCompleted = repaired >= 0;
                    result = Math.Max(0, repaired);
                    break;
                case GameAction.DODGE:
                    actionCompleted = attacker.Dodge();
                    break;
                case GameAction.ESCAPE:
                    actionCompleted = attacker.Escape();
                    if (actionCompleted)
                    {
                        EndByEscape();
                    }
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

       
        private static int ResolveAttack(int rawDamage, Robot defender, out bool completed)
        {
            completed = rawDamage >= 0;
            return completed ? defender.ReceiveDamage(rawDamage) : 0;
        }

        /**
         * Termine la partie suite à une fuite réussie : aucun gagnant.
         */
        public void EndByEscape()
        {
            escaped = true;
            status = GameStatus.END_GAME;
        }

        /**
         * Recopie l'état calculé par le serveur, sans aucun calcul de combat.
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
         * Vérifie si la partie est terminée (un robot détruit ou une fuite réussie).
         * @return true si la partie est terminée, false sinon.
         */
        public bool CheckGameEnded()
        {
            return escaped || robots.Any(r => !r.IsAlive());
        }

        /**
         * Retourne le gagnant de la partie, si la partie est terminée.
         * @return Le robot gagnant, ou null si la partie n'est pas terminée ou s'est terminée par une fuite.
         */
        public Robot? GetWinner()
        {
            if (escaped || !CheckGameEnded())
            {
                return null;
            }
            return robots.FirstOrDefault(r => r.IsAlive());
        }
    }
}
