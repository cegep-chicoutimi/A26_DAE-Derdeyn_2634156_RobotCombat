using System;
using System.Collections.Generic;
using System.Text;

namespace RobotCombat.Domain.Game
{
    /// <summary>
    /// Représente une statistique
    /// </summary>
    public class Stats(StatsType type, int baseValue)
    {
        /// <summary>
        /// Le type de la statistique
        /// </summary>
        public StatsType Type { get; } = type;
        /// <summary>
        /// La valeur actuelle de la statistique
        /// </summary>
        public int CurrentValue { get; set; } = baseValue;

        /// <summary>
        /// Augmente la valeur actuelle de la statistique d'une certaine valeur, sans dépasser une valeur maximale.
        /// </summary>
        /// <param name="value">La valeur à ajouter à la statistique.</param>
        /// <param name="maxValue">La valeur maximale que la statistique peut atteindre.</param>
        public void Increase(int value, int maxValue)
        {
            CurrentValue += value;
            if (CurrentValue > maxValue)
            {
                CurrentValue = maxValue;
            }
        }
        /// <summary>
        /// Diminue la valeur actuelle de la statistique d'une certaine valeur, sans descendre en dessous de zéro.
        /// </summary>
        /// <param name="value">La valeur à soustraire de la statistique.</param>
        public void Decrease(int amount) => CurrentValue = Math.Max(0, CurrentValue - amount);
        public override string ToString() => $"{Type} : {CurrentValue}";
    }   
}
