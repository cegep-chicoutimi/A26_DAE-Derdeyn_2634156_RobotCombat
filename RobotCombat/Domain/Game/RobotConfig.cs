using System;
using System.Collections.Generic;
using System.Text;

namespace RobotCombat.Domain.Game
{
    /// <summary>
    /// Représente la configuration d'un robot dans le jeu.
    /// </summary>
    public class RobotConfig
    {
        public int HpPoints { get; set; }
        public int ArmorPoints { get; set; }
        public int DamagePoints { get; set; }

        /// <summary>
        /// Vérifie si la configuration du robot est valide.
        /// </summary>
        /// <returns>true si la configuration est valide, sinon false.</returns>
        public bool IsValid()
        {
            return HpPoints >= 0 && ArmorPoints >= 0 && DamagePoints >= 0 && HpPoints + ArmorPoints + DamagePoints == 10;
        }

        /// <summary>
        /// Génère les statistiques du robot en fonction de sa configuration et des paramètres du jeu.
        /// </summary>
        /// <param name="config">La configuration du jeu.</param>
        /// <returns>Une liste de statistiques représentant les caractéristiques du robot.</returns>
        public List<Stats> GetStats(Config config)
        {
            return new List<Stats>
            {
                new Stats(StatsType.HP, config.BaseHp + HpPoints * config.HpPerPoint),
                new Stats(StatsType.ATTACK, config.BaseDamage + DamagePoints * config.DamagePerPoint),
                new Stats(StatsType.DEFENSE, config.BaseArmor + ArmorPoints * config.ArmorPerPoint),
                new Stats(StatsType.ENERGY, config.BaseEnergy)
            };
        }
    }
}
