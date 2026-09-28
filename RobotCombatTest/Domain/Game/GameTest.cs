namespace RobotCombatTest;

using RobotCombat.Domain;
using RobotCombat.Domain.Game;

[TestClass]
public class GameTest
{

    private static Robot CreateRobot(bool isPlayer = false)
    {
        var robotConfig = new RobotConfig
        {
            ArmorPoints = 0,
            DamagePoints = 0,
            HpPoints = 0
        };
        return new Robot(isPlayer, robotConfig, new Config());
    }

    private static Game CreateGame(out Robot host, out Robot player)
    {
        host = CreateRobot();
        player = CreateRobot(true);
        return new Game(new Config(), host, player);
    }


    [TestMethod]
    public void ShouldGenerateNonEmptyIdWhenGameIsCreated()
    {
        var game = CreateGame(out _, out _);

        Assert.IsFalse(string.IsNullOrEmpty(game.Id));
    }

    [TestMethod]
    public void ShouldGenerateDifferentIdsForTwoGames()
    {
        var game1 = CreateGame(out _, out _);
        var game2 = CreateGame(out _, out _);

        Assert.AreNotEqual(game1.Id, game2.Id);
    }

    [TestMethod]
    public void ShouldBeWaitingForPlayerConfigWhenGameIsCreated()
    {
        var game = CreateGame(out _, out _);

        Assert.AreEqual(GameStatus.WAITING_FOR_PLAYER_CONFIG, game.Status);
    }

    [TestMethod]
    public void ShouldContainTwoRobotsWhenGameIsCreated()
    {
        var game = CreateGame(out var host, out var player);

        Assert.AreEqual(2, game.robots.Count);
        Assert.AreSame(host, game.robots[0]);
        Assert.AreSame(player, game.robots[1]);
    }

    [TestMethod]
    public void ShouldLetHostPlayFirstWhenGameIsCreated()
    {
        var game = CreateGame(out var host, out var player);

        Assert.AreSame(host, game.CurrentRobot);
        Assert.AreSame(player, game.OpponentRobot);
    }


}