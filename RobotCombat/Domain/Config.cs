namespace RobotCombat.Domain
{
    /// <summary>
    /// Représente la configuration du jeu.
    /// </summary>
    public record Config(
        int Port = 3000,
        string IpAddress = "127.0.0.1", // affichage uniquement
        int MaxPlayers = 1,
        int PointsToGive = 10,
        int BaseEnergy = 2,
        int MaxEnergy = 5,
        int BaseHp = 100,
        int BaseArmor = 0,
        int BaseDamage = 10,
        int HpPerPoint = 10,
        int ArmorPerPoint = 2,
        int DamagePerPoint = 2,
       
        int RechargeEnergyGain = 2,
        int DefenseMinBonus = 5,
        int LuckToEscapePercent = 15,
        int RepairPercent = 10,
        int RepairMinHp = 5,

        int MinSuccessPercent = 50,
        int MaxSuccessPercent = 85,
        int MaxFailStreak = 2,
        int RepairEnergyCost = 1,
        int DodgeCostEnergy =1,

        int DefenseBonusPercent = 40,
        int PowerDamageEnergyCost = 2,
        int PowerDamageMultiplier = 2
    )
    {
    }
}
