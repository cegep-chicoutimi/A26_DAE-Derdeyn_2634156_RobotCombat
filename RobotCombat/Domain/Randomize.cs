namespace RobotCombat.Domain
{
    public class Randomize(Config config)
    {
        private readonly Random _random = new Random();

        /**
         * Vérifie si une action réussit en fonction des points de vie du robot.
         * Plus le robot est affaibli, plus il a de chances de réussir.
         * La chance monte linéairement de MinSuccessPercent (pleine vie) à MaxSuccessPercent (0 PV) :
         * chance = Min + (Max - Min) × PV perdus / PV max
         * (défaut : 100 % PV => 60 %, 50 % PV => ~77 %, 10 % PV => ~91 %)
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
            int lostHp = Math.Clamp(hpBase - hpNow, 0, hpBase);
            int chancePercent = config.MinSuccessPercent + (config.MaxSuccessPercent - config.MinSuccessPercent) * lostHp / hpBase;
            return _random.Next(0, 100) < chancePercent;
        }

        /**
         * Vérifie si le robot réussit à s'échapper.
         * @return true si le robot s'échappe, sinon false
         */
        public bool RandomEscape() => _random.Next(0, 100) < config.LuckToEscapePercent;
    }
}
