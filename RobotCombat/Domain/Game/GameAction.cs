namespace RobotCombat.Domain.Game
{
    /// <summary>
    /// Représente les différentes actions possibles dans le jeu par un robot
    /// </summary>
    public enum GameAction
    {
        ATTACK,
        DEFENSE,
        ATTACK_PUISSANCE,
        RECHARGE,
        REPAIR,
        DODGE,
        ESCAPE
    }
    public static class GameActionCompanion
    {


        /// <summary>
        /// Fournit une description du résultat d'une action de jeu.
        /// </summary>
        /// <param name="action">L'action effectuée.</param>
        /// <param name="completed">true si l'action a réussi (actionCompleted).</param>
        /// <param name="value">Dégâts infligés (attaques) ou PV récupérés (réparation).</param>
        /// <returns>Une chaîne de caractères décrivant le résultat de l'action.</returns>
        public static string ResultOfAction(this GameAction action, bool completed, int value)
        {
            return action switch
            {
                GameAction.ATTACK or GameAction.ATTACK_PUISSANCE when !completed => "Attaque ratée",
                GameAction.ATTACK or GameAction.ATTACK_PUISSANCE when value == 0 => "Attaque esquivée par la cible",
                GameAction.ATTACK => $"Dégâts infligés : {value}",
                GameAction.ATTACK_PUISSANCE => $"Dégâts puissants infligés : {value}",

                GameAction.DEFENSE => completed ? "Bouclier activé pour la prochaine attaque reçue" : "Défense ratée",

                GameAction.RECHARGE => "Rechargement de l'énergie",

                GameAction.REPAIR => completed ? $"Réparation réussie" : "Réparation ratée",

                GameAction.DODGE => completed ? "Esquive prête : la prochaine attaque sera évitée" : "Esquive ratée",

                GameAction.ESCAPE => completed ? "Fuite réussie : fin de la partie, aucun gagnant" : "Fuite ratée",

                _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
            };

        }

        /// <summary>
        /// Libellé lisible d'une action (affichage du menu).
        /// NB : une extension nommée ToString n'est jamais appelée (Enum.ToString est prioritaire),
        /// d'où le nom ToLabel.
        /// </summary>
        public static string ToLabel(this GameAction action)
        {
            return action switch
            {
                GameAction.ATTACK => "ATTAQUE",
                GameAction.DEFENSE => "DÉFENSE",
                GameAction.ATTACK_PUISSANCE => "ATTAQUE PUISSANTE",
                GameAction.RECHARGE => "RECHARGE",
                GameAction.REPAIR => "RÉPARATION",
                GameAction.DODGE => "ESQUIVE",
                GameAction.ESCAPE => "FUITE",
                _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
            };
        }
    }
}
