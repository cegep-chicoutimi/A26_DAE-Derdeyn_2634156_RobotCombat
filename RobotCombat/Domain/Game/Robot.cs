namespace RobotCombat.Domain.Game
{
    /**
      * Représente un robot dans le jeu.
      * Contient les statistiques du robot et les actions qu'il peut effectuer.
      */
    public class Robot(bool isHost, RobotConfig robotConfig, Config config, Randomize randomize)
    {
        public readonly bool IsHost = isHost;

        private bool isDefending = false;
        // Esquive active : la PROCHAINE attaque reçue est évitée
        private bool isDodging = false;
        // Nombre d'actions ratées d'affilée (anti-malchance)
        private int failStreak = 0;

        private readonly List<Stats> stats = robotConfig.GetStats(config);

        private int MaxHp => config.BaseHp + robotConfig.HpPoints * config.HpPerPoint;

        /**
         * Vérifie si le robot est encore en vie
         * @return true si le robot est vivant sinon false
         */
        public bool IsAlive()
        {
            return GetStat(StatsType.HP).CurrentValue > 0;
        }

        /**
         * Effectue une attaque normale et retourne les dégâts infligés.
         * @return Les dégâts infligés par l'attaque ou -1 si l'attaque a échoué.
         */
        public int Attack()
        {
            if (!HasCompleted())
            {
                return -1;
            }

            return GetStat(StatsType.ATTACK).CurrentValue;
        }

        /**
        * Vérifie si une attaque puissante est possible.
        * @return Vrai si l'attaque puissante est possible d'être effectuée, sinon false.
        */
        public bool CanAttackWithPower() => GetStat(StatsType.ENERGY).CurrentValue >= config.PowerDamageEnergyCost;

        /**
         * Effectue une attaque puissante (coûte de l'énergie, dégâts multipliés).
         * @return Les dégâts bruts de l'attaque, ou -1 si l'attaque a échoué.
         * @throws InvalidOperationException si l'énergie est insuffisante.
         */
        public int AttackWithPower()
        {
            if (!CanAttackWithPower())
            {
                throw new InvalidOperationException("Énergie insuffisante pour une attaque puissante.");
            }
            if (!HasCompleted())
            {
                return -1;
            }
            ResetDefend();
            GetStat(StatsType.ENERGY).Decrease(config.PowerDamageEnergyCost);
            return GetStat(StatsType.ATTACK).CurrentValue * config.PowerDamageMultiplier;
        }

        /**
         * Recharge l'énergie du robot.
         */
        public void Recharge()
        {
            GetStat(StatsType.ENERGY).Increase(config.RechargeEnergyGain, config.MaxEnergy);
        }

        /**
         * Active le bonus de défense pour la prochaine attaque reçue.
         * @return true si la défense a réussi, sinon false.
         */
        public bool Defend()
        {
            if (!HasCompleted())
            {
                return false;
            }
            isDefending = true;
            return true;
        }

        /**
         * Tente de réparer le robot (pourcentage des PV actuels, avec un minimum), sans dépasser les PV max.
         * @return Le nombre de PV réellement récupérés, ou -1 si la réparation a échoué.
         */
        public int Repair()
        {
            if (!HasCompleted())
            {
                return -1;
            }
            Stats hp = GetStat(StatsType.HP);
            int before = hp.CurrentValue;
            int repairAmount = Math.Max(config.RepairMinHp, before * config.RepairPercent / 100);
            hp.Increase(repairAmount, MaxHp);
            return hp.CurrentValue - before;
        }

        /**
         * Tente de préparer une esquive : si elle réussit, la prochaine attaque reçue est évitée.
         * @return true si l'esquive est prête, sinon false.
         */
        public bool Dodge()
        {
            if (!HasCompleted())
            {
                return false;
            }
            isDodging = true;
            return true;
        }

        /**
         * Tente de fuir le combat (chance fixe définie dans la config).
         * @return true si la fuite a réussi (fin de partie sans gagnant), sinon false.
         */
        public bool Escape()
        {
            return randomize.RandomEscape();
        }

        /**
         * Inflige des dégâts au robot en tenant compte de son armure, de son bonus de défense et de son esquive.
         * Le bonus de défense et l'esquive retombent dès qu'une attaque est reçue.
         * @param damage Les dégâts bruts à infliger.
         * @return Les dégâts effectivement infligés (0 si l'attaque a été esquivée).
         */
        public int ReceiveDamage(int damage)
        {
            if (isDodging)
            {
                isDodging = false;
                return 0;
            }

            int mitigation = GetStat(StatsType.DEFENSE).CurrentValue;
            if (isDefending)
            {
                // Bonus = 40 % de l'armure, avec un minimum de 5
                mitigation += Math.Max(config.DefenseMinBonus, config.DefenseBonusPercent * mitigation / 100);
                isDefending = false;
            }

            int actualDamage = Math.Max(1, damage - mitigation);
            GetStat(StatsType.HP).Decrease(actualDamage);
            return actualDamage;
        }

        /**
         * Retourne une statistique précise actuelle du robot sous forme de chaîne de caractères.
         * @param type Le type de statistique à récupérer.
         * @return La valeur actuelle de la statistique sous forme de chaîne.
         */
        public string GetStats(StatsType type)
        {
            return GetStat(type).CurrentValue.ToString();
        }

        /**
         * Recopie les PV et l'énergie envoyés par le serveur.
         */
        public void ForceStat(int hp, int energy)
        {
            GetStat(StatsType.HP).CurrentValue = hp;
            GetStat(StatsType.ENERGY).CurrentValue = energy;
        }

        private Stats GetStat(StatsType type)
        {
            return stats.First(s => s.Type == type);
        }

        /**
         * Détermine si l'action réussit : plus le robot est affaibli, plus il a de chances de réussir.
         */
        private bool HasCompleted()
        {
            // Anti-malchance : après MaxFailStreak échecs d'affilée, l'action réussit forcément
            bool completed = failStreak >= config.MaxFailStreak
                          || randomize.HasCompleteRandom(MaxHp, GetStat(StatsType.HP).CurrentValue);

            failStreak = completed ? 0 : failStreak + 1;
            return completed;
        }

        

        private void ResetDodging()
        {
            isDodging = false;
        }

        private void ResetDefend()
        {
            isDefending = false;
        }
    }
}
