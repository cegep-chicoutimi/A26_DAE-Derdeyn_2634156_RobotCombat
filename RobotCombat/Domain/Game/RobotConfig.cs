using System;
using System.Collections.Generic;
using System.Text;

namespace RobotCombat.Domain.Game
{
    /**
     * Représente la configuration d'un robot dans le jeu.
     */
    public class RobotConfig
    {
        public int HpPoints { get; set; }
        public int ArmorPoints { get; set; }
        public int DamagePoints { get; set; }

        /**
         * Vérifie si la configuration du robot est valide.
         * @return true si la configuration est valide, sinon false.
         */
        public bool IsValid()
        {
            return HpPoints >= 0 && ArmorPoints >= 0 && DamagePoints >= 0 && HpPoints+ ArmorPoints+ DamagePoints == 10;
        }
    }
}
