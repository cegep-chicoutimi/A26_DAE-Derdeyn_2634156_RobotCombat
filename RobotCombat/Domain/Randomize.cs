namespace RobotCombat.Domain
{
    public class Randomize(Config config)
    {
        private readonly Random _random = new Random();

        /// <summary>
        /// Vérifie si une action réussit en fonction des points de vie du robot.
        /// Plus le robot est affaibli, plus il a de chances de réussir.
        /// La chance monte linéairement de MinSuccessPercent (pleine vie) à MaxSuccessPercent (0 PV)
        /// </summary>
        /// <param name="hpBase">Les points de vie maximum du robot.</param>
        /// <param name="hpNow">Les points de vie actuels du robot.</param>
        /// <returns>true si l'action réussit, sinon false</returns>
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

        /// <summary>
        /// Vérifie si le robot réussit à s'échapper.
        /// </summary>
        /// <returns>true si le robot s'échappe, sinon false</returns>
        public bool RandomEscape() => _random.Next(0, 100) < config.LuckToEscapePercent;
    }
}
