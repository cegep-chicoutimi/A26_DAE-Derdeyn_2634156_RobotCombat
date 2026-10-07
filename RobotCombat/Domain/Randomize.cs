namespace RobotCombat.Domain
{
    public class Randomize(Config config)
    {
        private readonly Random _random = new Random();

        /**
         * Vérifie si une action réussit en fonction des points de vie du robot.
         * Chance de réussite = hpNow / hpBase (100 % à pleine vie).
         * @param hpBase Les points de vie maximum du robot.
         * @param hpNow Les points de vie actuels du robot.
         * @return true si l'action réussit, sinon false
         */
        public bool HasCompleteRandom(int hpBase, int hpNow)
        {
            if (hpBase <= 0)
            {
                return true;
            }
            return _random.Next(0, hpBase) < hpNow;
        }
        /**
         * Vérifie si le robot réussit à s'échapper.
         * @return true si le robot s'échappe, sinon false
         */
        public bool RandomEscape() => _random.Next(0, 100) < config.LuckToEscapePercent;
    }
}
