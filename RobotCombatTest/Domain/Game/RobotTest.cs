namespace RobotCombatTest;

using RobotCombat.Domain;
using RobotCombat.Domain.Game;

[TestClass]
public class RobotTest
{
    private static Robot CreateRobot(int hpPoints = 0, int armorPoints = 0, int damagePoints = 0)
    {
        var robotConfig = new RobotConfig { HpPoints = hpPoints, ArmorPoints = armorPoints, DamagePoints = damagePoints };
        var config = new Config(MinSuccessPercent: 100, MaxSuccessPercent: 100);
        return new Robot(false, robotConfig, config, new Randomize(config));
    }

    [TestMethod]
    public void ShouldLoseHpWhenReceivingDamage()
    {
        var robot = CreateRobot();

        robot.ReceiveDamage(20);

        Assert.AreEqual("80", robot.GetStats(StatsType.HP));
    }

    [TestMethod]
    public void ShouldDieWhenHpIsZero()
    {
        var robot = CreateRobot();

        Assert.IsTrue(robot.IsAlive());

        robot.ReceiveDamage(200);
        Assert.AreEqual("0", robot.GetStats(StatsType.HP));
        Assert.IsFalse(robot.IsAlive());
    }

    [TestMethod]
    public void ShouldAlwaysInflictAtLeastOneDamage()
    {
        var robot = CreateRobot(armorPoints: 10);

        int damage = robot.ReceiveDamage(5);

        Assert.AreEqual(1, damage);
        Assert.AreEqual("99", robot.GetStats(StatsType.HP));
    }

    [TestMethod]
    public void ShouldAvoidNextAttackOnlyWhenDodging()
    {
        var robot = CreateRobot();

        Assert.IsTrue(robot.Dodge());
        Assert.AreEqual(0, robot.ReceiveDamage(30));
        Assert.AreEqual("100", robot.GetStats(StatsType.HP));

        Assert.AreEqual(30, robot.ReceiveDamage(30));
    }

    [TestMethod]
    public void ShouldReduceNextAttackWhenDefending()
    {
        var robot = CreateRobot(armorPoints: 10);

        Assert.IsTrue(robot.Defend());
        Assert.AreEqual(2, robot.ReceiveDamage(30));
     
        Assert.AreEqual(10, robot.ReceiveDamage(30));
    }

    [TestMethod]
    public void ShouldApplyMinimumDefenseBonusWhenArmorIsLow()
    {
        var robot = CreateRobot(); 

        Assert.IsTrue(robot.Defend());
        Assert.AreEqual(25, robot.ReceiveDamage(30));
      
        Assert.AreEqual(30, robot.ReceiveDamage(30));
    }

    [TestMethod]
    public void ShouldNotRepairAboveMaxHp()
    {
        var robot = CreateRobot();

        Assert.AreEqual(0, robot.Repair());
        Assert.AreEqual("100", robot.GetStats(StatsType.HP));
    }

    [TestMethod]
    public void ShouldRepairAtLeastMinimumHp()
    {
        var robot = CreateRobot();
        robot.ForceStat(1, 2);

        Assert.AreEqual(5, robot.Repair());
    }

    [TestMethod]
    public void ShouldNotRechargeAboveMaxEnergy()
    {
        var robot = CreateRobot();

        for (int i = 0; i < 10; i++)
        {
            robot.Recharge();
        }

        Assert.AreEqual("5", robot.GetStats(StatsType.ENERGY));
    }
}
