namespace RobotCombatTest;

using RobotCombat.Domain;
using RobotCombat.Domain.Game;

[TestClass]
public class RobotTest
{
    [TestMethod]
    public void ShouldLoseHpWhenReceivingDamage()
    {
        var robotConfig = new RobotConfig();
        robotConfig.ArmorPoints = 0;
        robotConfig.DamagePoints = 0;
        robotConfig.HpPoints = 0;

        var config = new Config();
        var robot = new Robot(false, robotConfig, config);

        robot.ReceiveDamage(20);

        Assert.AreEqual(robot.GetStats(StatsType.HP), "80");
    }

    [TestMethod]
    public void ShouldDieWhenHpIsZero()
    {
        var robotConfig = new RobotConfig();
        robotConfig.ArmorPoints = 0;
        robotConfig.DamagePoints = 0;
        robotConfig.HpPoints = 0;

        var config = new Config();
        var robot = new Robot(false, robotConfig, config);

        Assert.IsTrue(robot.IsAlive());

        robot.ReceiveDamage(200);
        Assert.AreEqual(robot.GetStats(StatsType.HP), "0");
        Assert.IsFalse(robot.IsAlive());
    }
}