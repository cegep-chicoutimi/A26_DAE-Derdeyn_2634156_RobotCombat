using System;
using System.Collections.Generic;
using System.Text;

namespace RobotCombat.Domain
{
   
    public class Randomize(Config config)
    {
        private readonly Random _random = new Random();

        /**
         * Vérifie si le robot a une chance de réussir une action aléatoire en fonction de ses points de vie.
         * @param hpBase Les points de vie de base du robot.
         * @param hpNow Les points de vie actuels du robot.
         * @return true si le robot a une chance de réussir l'action, sinon false
         */
        public bool HasCompleteRandom(int hpBase, int hpNow)
        {
            return _random.Next(0, hpBase) < hpNow;
        }
        /**
         * Vérifie si le robot peut s'échapper en fonction de sa chance.
         * @return true si le robot peut s'échapper, sinon false
         */
        public bool RandomEscape() => _random.Next(0, 100) <= config.LuckToEscapePercent;
    }
}
