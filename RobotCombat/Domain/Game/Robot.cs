using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace RobotCombat.Domain.Game
{
    /**
     * Représente un robot dans le jeu.
     * Contient les statistiques du robot et les actions qu'il peut effectuer.
     */
    public class Robot(Boolean isHost, RobotConfig robotConfig, Config config)
    {
        public readonly Boolean IsHost = isHost;
        private Boolean isRecharging = false;
        private Boolean isDefending = false; // mieux d'enregister la derniere action effectuee et de calculer les stats en fonction de ca ?
        private readonly List<Stats> stats = new List<Stats>
            {
                new Stats(StatsType.HP, config.BaseHp + robotConfig.HpPoints * config.HpPerPoint),
                new Stats(StatsType.ATTACK, config.BaseDamage +robotConfig.DamagePoints * config.DamagePerPoint),
                new Stats(StatsType.DEFENSE, config.BaseArmor+robotConfig.ArmorPoints * config.ArmorPerPoint),
                new Stats(StatsType.ENERGY, config.BaseEnergy)
            };

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
         * @return Les dégâts infligés par l'attaque.
         */
        public int Attack()
        {
            ResetActions();
            return GetStat(StatsType.ATTACK).CurrentValue;
        }
        /**
         * Effectue une attaque puissante si le robot a assez d'énergie.
         * @return Les dégâts infligés par l'attaque puissante, ou 0 si pas assez d'énergie.
         */
        public bool CanAttackWithPower() => GetStat(StatsType.ENERGY).CurrentValue >= config.PowerDamageEnergyCost;

        public int AttackWithPower()
        {
            if (!CanAttackWithPower())
            {
                throw new InvalidOperationException("Énergie insuffisante pour une attaque puissante.");
            }
            isRecharging = false;
            GetStat(StatsType.ENERGY).Decrease(config.PowerDamageEnergyCost);
            return GetStat(StatsType.ATTACK).CurrentValue * config.PowerDamageMultiplier;
        }
        /**
         * Recharge l'énergie du robot.
         */
        public void Recharge()
        {
            ResetDefend();
            isRecharging = true;
            GetStat(StatsType.ENERGY).Increase(config.RechargeEnergyGain, config.MaxEnergy);
        }
        /**
         * Défend le robot, augmentant sa défense pour le prochain tour.
         */
        public void Defend()
        {
            ResetRecharge();
            isDefending = true;
        }
        /**
         * Inflige des dégâts au robot en tenant compte de sa défense et de son état de défense.
         * @param damage Les dégâts à infliger.
         * @return Les dégâts effectivement infligés.
         */
        public int ReceiveDamage(int damage)
        {
            int mitigation = GetStat(StatsType.DEFENSE).CurrentValue;
            if (isDefending)
            {
                mitigation += config.DefenseBonus;
            }

            int actualDamage = Math.Max(1, damage - mitigation);
            GetStat(StatsType.HP).Decrease(actualDamage);
            ResetDefend(); 
            return actualDamage;
        }
        /**
         * Retourne une statistique précise actuelles du robot sous forme de chaîne de caractères.
         * @param type Le type de statistique à récupérer.
         * @return La valeur actuelle de la statistique sous forme de chaîne.
         */
        public string GetStats(StatsType type)
        {
            return GetStat(type).CurrentValue.ToString();
        }

        /**
         * (Client) Recopie les PV et l'énergie envoyés par le serveur.
         */
        public void ForceStat(int hp, int energy)
        {
            GetStat(StatsType.HP).CurrentValue = hp;
            GetStat(StatsType.ENERGY).CurrentValue = energy;
        }

        private void ResetRecharge()
        {
            isRecharging = false;
        }
        private void ResetDefend()
        {
            isDefending = false;
        }
        private void ResetActions()
        {
            ResetRecharge();
            ResetDefend();
        }

        private Stats GetStat(StatsType type)
        {
            return stats.First(s => s.Type == type);
        }

    }
}
