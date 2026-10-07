namespace RobotCombatTest;

using RobotCombat.Domain;
using RobotCombat.Domain.Game;

[TestClass]
public class RobotTest
{
    private static Robot CreateRobot(int hpPoints = 0, int armorPoints = 0, int damagePoints = 0)
    {
        var robotConfig = new RobotConfig { HpPoints = hpPoints, ArmorPoints = armorPoints, DamagePoints = damagePoints };
        var config = new Config(MinSuccessPercent: 100); // réussite garantie => tests déterministes
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
        var robot = CreateRobot(armorPoints: 10); // armure 20

        int damage = robot.ReceiveDamage(5);

        Assert.AreEqual(1, damage);
        Assert.AreEqual("99", robot.GetStats(StatsType.HP));
    }

    [TestMethod]
    public void ShouldAvoidNextAttackOnlyWhenDodging()
    {
        var robot = CreateRobot();

        Assert.IsTrue(robot.Dodge()); // pleine vie => réussite garantie
        Assert.AreEqual(0, robot.ReceiveDamage(30));
        Assert.AreEqual("100", robot.GetStats(StatsType.HP));

        // L'esquive est consommée : l'attaque suivante touche
        Assert.AreEqual(30, robot.ReceiveDamage(30));
    }

    [TestMethod]
    public void ShouldReduceNextAttackWhenDefending()
    {
        var robot = CreateRobot(armorPoints: 10); // armure 20, bonus 40% = 8 => 28

        Assert.IsTrue(robot.Defend());
        Assert.AreEqual(2, robot.ReceiveDamage(30));
        // Le bonus retombe après l'attaque reçue
        Assert.AreEqual(10, robot.ReceiveDamage(30));
    }

    [TestMethod]
    public void ShouldApplyMinimumDefenseBonusWhenArmorIsLow()
    {
        var robot = CreateRobot(); // armure 0, 40% = 0 => bonus minimum 5

        Assert.IsTrue(robot.Defend());
        Assert.AreEqual(25, robot.ReceiveDamage(30));
        // Le bonus retombe après l'attaque reçue
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

        // 10% de 1 PV = 0 => minimum 5
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

[TestClass]
public class RandomizeTest
{
    private static int CountSuccesses(Randomize randomize, int hpBase, int hpNow, int tries = 10000)
    {
        int successes = 0;
        for (int i = 0; i < tries; i++)
        {
            if (randomize.HasCompleteRandom(hpBase, hpNow))
            {
                successes++;
            }
        }
        return successes;
    }

    [TestMethod]
    public void ShouldAlwaysSucceedWhenAlmostDead()
    {
        var randomize = new Randomize(new Config());
        Assert.AreEqual(1000, CountSuccesses(randomize, 100, 0, 1000));
    }

    [TestMethod]
    public void ShouldUseMinimumChanceAtFullHp()
    {
        var randomize = new Randomize(new Config()); // MinSuccessPercent = 30
        int successes = CountSuccesses(randomize, 100, 100);
        Assert.IsTrue(successes > 2500 && successes < 3500, $"Réussites : {successes}");
    }

    [TestMethod]
    public void ShouldSucceedMoreWhenWeaker()
    {
        var randomize = new Randomize(new Config());
        int at20Percent = CountSuccesses(randomize, 100, 20); // ~80 %
        Assert.IsTrue(at20Percent > 7500 && at20Percent < 8500, $"Réussites : {at20Percent}");
    }

    [TestMethod]
    public void ShouldAlwaysFailAtFullHpWithoutMinimum()
    {
        var randomize = new Randomize(new Config(MinSuccessPercent: 0));
        Assert.AreEqual(0, CountSuccesses(randomize, 100, 100, 1000));
    }
}
