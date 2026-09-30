using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace RobotCombat.Domain.Game
{
    /**
     * Représente les différentes actions possibles dans le jeu par un robot
     */
    public enum GameAction
    {
        ATTACK,
        DEFENSE,
        ATTACK_PUISSANCE,
        RECHARGE
    }
    public static class GameActionCompanion
    {
        ///**
        // * Convertit une action de jeu en chaîne de caractères.
        // * @param action L'action à convertir.
        // * @return La chaîne de caractères représentant l'action.
        // */
        //public static string ToString(this GameAction action)
        //{
        //    return action switch
        //    {
        //        GameAction.ATTACK => "Attaque",
        //        GameAction.DEFENSE => "Défense",
        //        GameAction.ATTACK_PUISSANCE => "Attaque Puissance",
        //        GameAction.RECHARGE => "Recharge",
        //        _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
        //    };
        //}


        /**
         * Fournit une description du résultat d'une action de jeu.
         * @param action L'action effectuée.
         * @param result Le résultat de l'action (par exemple, les dégâts infligés).
         * @return Une chaîne de caractères décrivant le résultat de l'action.
         */
        public static string ResultOfAction(this GameAction action, int result)
        {
            return action switch
            {
                GameAction.ATTACK => $"Dégâts infligés : {result}",

                GameAction.DEFENSE => "Bouclier temporaire activé",

                GameAction.ATTACK_PUISSANCE when result == 0 => "Dégâts puissants manqués",

                GameAction.ATTACK_PUISSANCE => $"Dégâts puissants infligés : {result}",

                GameAction.RECHARGE =>  $"Rechargement de l'énergie",

                _ =>
                    throw new ArgumentOutOfRangeException(nameof(action), action, null)
            };
        }

        public static string ToString(this GameAction action)
        {
            return action switch
            {
                GameAction.ATTACK => "ATTACK",
                GameAction.DEFENSE => "DEFENSE",
                GameAction.ATTACK_PUISSANCE => "ATTACK_PUISSANCE",
                GameAction.RECHARGE => "RECHARGE",
                _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
            };

        }
    }
}
