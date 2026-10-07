namespace RobotCombat.Domain
{
    /**
     * Représente la configuration du jeu.
     */
    public record Config(
        int Port = 3000,
        string IpAddress = "192.168.2.173", // affichage uniquement
        int MaxPlayers = 1,
        int PointsToGive = 10,
        int BaseEnergy = 2,
        int MaxEnergy = 5,
        int PowerDamageEnergyCost = 2,
        int PowerDamageMultiplier = 2,
        int RechargeEnergyGain = 1,
        int BaseHp = 100,
        int BaseArmor = 0,
        int BaseDamage = 10,
        int HpPerPoint = 10,
        int ArmorPerPoint = 2,
        int DamagePerPoint = 2,
        int DefenseBonusPercent = 40,
        int DefenseMinBonus = 5,
        int LuckToEscapePercent = 15,
        int RepairPercent = 10,
        int RepairMinHp = 5,
        int MinSuccessPercent = 30
    )
    {
    }
}
