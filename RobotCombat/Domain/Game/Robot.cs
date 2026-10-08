namespace RobotCombat.Domain.Game
{
    /// <summary>
    /// Représente un robot dans le jeu.
    /// Contient les statistiques du robot et les actions qu'il peut effectuer.
    /// </summary>
    public class Robot(bool isHost, RobotConfig robotConfig, Config config, Randomize randomize)
    {
        public readonly bool IsHost = isHost;

        private bool isDefending = false;

        private bool isDodging = false;

        private int failStreak = 0;

        private readonly List<Stats> stats = robotConfig.GetStats(config);

        private int MaxHp => config.BaseHp + robotConfig.HpPoints * config.HpPerPoint;

        /// <summary>
        /// Vérifie si le robot est encore en vie
        /// </summary>
        /// <returns>true si le robot est vivant sinon false</returns>
        public bool IsAlive()
        {
            return GetStat(StatsType.HP).CurrentValue > 0;
        }

        /// <summary>
        /// Effectue une attaque normale et retourne les dégâts infligés.
        /// </summary>
        /// <returns>Les dégâts infligés par l'attaque ou -1 si l'attaque a échoué.</returns>
        public int Attack()
        {
            if (!HasCompleted())
            {
                return -1;
            }

            return GetStat(StatsType.ATTACK).CurrentValue;
        }

        /// <summary>
        /// Vérifie si une attaque puissante est possible.
        /// </summary>
        /// <returns>Vrai si l'attaque puissante est possible d'être effectuée, sinon false.</returns>
        public bool CanAttackWithPower() => GetStat(StatsType.ENERGY).CurrentValue >= config.PowerDamageEnergyCost;

        public bool CanRepair() => GetStat(StatsType.ENERGY).CurrentValue >= config.RepairEnergyCost;
        public bool CanDodge() => GetStat(StatsType.ENERGY).CurrentValue >= config.DodgeCostEnergy;
        

        /// <summary>
        /// Effectue une attaque puissante (coûte de l'énergie, dégâts multipliés).
        /// </summary>
        /// <returns>Les dégâts bruts de l'attaque, ou -1 si l'attaque a échoué.</returns>
        /// <exception cref="InvalidOperationException">si l'énergie est insuffisante.</exception>
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

        /// <summary>
        /// Recharge l'énergie du robot.
        /// </summary>
        public void Recharge()
        {
            GetStat(StatsType.ENERGY).Increase(config.RechargeEnergyGain, config.MaxEnergy);
        }

        /// <summary>
        /// Active le bonus de défense pour la prochaine attaque reçue.
        /// </summary>
        /// <returns>true si la défense a réussi, sinon false.</returns>
        public bool Defend()
        {
            if (!HasCompleted())
            {
                return false;
            }
            isDefending = true;
            return true;
        }

        /// <summary>
        /// Tente de réparer le robot (pourcentage des PV actuels, avec un minimum), sans dépasser les PV max.
        /// </summary>
        /// <returns>Le nombre de PV réellement récupérés, ou -1 si la réparation a échoué.</returns>
        public int Repair()
        {
            if (!CanRepair())
            {
                throw new InvalidOperationException("Énergie insuffisante pour une attaque puissante.");
            }
            if (!HasCompleted())
            {
                return -1;
            }
            Stats hp = GetStat(StatsType.HP);
            Stats energy = GetStat(StatsType.ENERGY);
            energy.Decrease(config.RepairEnergyCost);
            int before = hp.CurrentValue;
            int repairAmount = Math.Max(config.RepairMinHp, before * config.RepairPercent / 100);
            hp.Increase(repairAmount, MaxHp);
            return hp.CurrentValue - before;
        }

        /// <summary>
        /// Tente de préparer une esquive : si elle réussit, la prochaine attaque reçue est évitée.
        /// </summary>
        /// <returns>true si l'esquive est prête, sinon false.</returns>
        public bool Dodge()
        {
            if (!CanDodge())
            {
                throw new InvalidOperationException("Énergie insuffisante pour une esquiver.");
            }
            if (!HasCompleted())
            {
                return false;
            }
            isDodging = true;
            return true;
        }

        /// <summary>
        /// Tente de fuir le combat (chance fixe définie dans la config).
        /// </summary>
        /// <returns>true si la fuite a réussi (fin de partie sans gagnant), sinon false.</returns>
        public bool Escape()
        {
            return randomize.RandomEscape();
        }

        /// <summary>
        /// Inflige des dégâts au robot en tenant compte de son armure, de son bonus de défense et de son esquive.
        /// Le bonus de défense et l'esquive retombent dès qu'une attaque est reçue.
        /// </summary>
        /// <param name="damage">Les dégâts bruts à infliger.</param>
        /// <returns>Les dégâts effectivement infligés (0 si l'attaque a été esquivée).</returns>
        public int ReceiveDamage(int damage)
        {
            if (isDodging)
            {
                isDodging = false;
                return 0;
            }

            int defValue = GetStat(StatsType.DEFENSE).CurrentValue;
            if (isDefending)
            {
                // Bonus = 40 % de l'armure, avec un minimum de 5
                defValue += Math.Max(config.DefenseMinBonus, config.DefenseBonusPercent * defValue / 100);
                isDefending = false;
            }

            int actualDamage = Math.Max(1, damage - defValue);
            GetStat(StatsType.HP).Decrease(actualDamage);
            return actualDamage;
        }

        /// <summary>
        /// Retourne une statistique précise actuelle du robot sous forme de chaîne de caractères.
        /// </summary>
        /// <param name="type">Le type de statistique à récupérer.</param>
        /// <returns>La valeur actuelle de la statistique sous forme de chaîne.</returns>
        public string GetStats(StatsType type)
        {
            return GetStat(type).CurrentValue.ToString();
        }

        /// <summary>
        /// Recopie les PV et l'énergie envoyés par le serveur.
        /// </summary>
        public void ForceStat(int hp, int energy)
        {
            GetStat(StatsType.HP).CurrentValue = hp;
            GetStat(StatsType.ENERGY).CurrentValue = energy;
        }

        private Stats GetStat(StatsType type)
        {
            return stats.First(s => s.Type == type);
        }

        /// <summary>
        /// Détermine si l'action réussit : plus le robot est affaibli, plus il a de chances de réussir.
        /// </summary>
        private bool HasCompleted()
        {
            bool completed = failStreak >= config.MaxFailStreak  || randomize.HasCompleteRandom(MaxHp, GetStat(StatsType.HP).CurrentValue);

            failStreak = completed ? 0 : failStreak + 1;
            return completed;
        }


        private void ResetDefend()
        {
            isDefending = false;
        }
    }
}
