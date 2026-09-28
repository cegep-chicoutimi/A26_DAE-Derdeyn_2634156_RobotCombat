using System;
using System.Collections.Generic;
using System.Text;

namespace RobotCombat.Domain.Game
{
    /**
     * Représente une statistique
     */
    public class Stats(StatsType type, int baseValue)
    {
        /**
         * Le type de la statistique
         */
        public StatsType Type { get; } = type;
        /**
         * La valeur actuelle de la statistique
         */
        public int CurrentValue { get; set; } = baseValue;

        /**
         * Augmente la valeur actuelle de la statistique d'une certaine valeur, sans dépasser une valeur maximale.
         * @param value La valeur à ajouter à la statistique.
         * @param maxValue La valeur maximale que la statistique peut atteindre.
         */
        public void Increase(int value, int maxValue)
        {
            CurrentValue += value;
            if (CurrentValue > maxValue)
            {
                CurrentValue = maxValue;
            }
        }
        /**
         * Diminue la valeur actuelle de la statistique d'une certaine valeur, sans descendre en dessous de zéro.
         * @param value La valeur à soustraire de la statistique.
         */
        public void Decrease(int amount) => CurrentValue = Math.Max(0, CurrentValue - amount);
        public override string ToString() => $"{Type} : {CurrentValue}";
    }   
}
