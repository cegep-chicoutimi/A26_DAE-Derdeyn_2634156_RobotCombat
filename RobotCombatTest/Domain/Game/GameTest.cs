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
        var config = new Config(MinSuccessPercent: 100, MaxSuccessPercent: 100); // réussite garantie => tests déterministes
        return new Robot(isPlayer, robotConfig, config, new Randomize(config));
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

    [TestMethod]
    public void ShouldEndWithoutWinnerWhenRobotEscapes()
    {
        var game = CreateGame(out _, out _);
        game.StartGame();

        game.EndByEscape();

        Assert.AreEqual(GameStatus.END_GAME, game.Status);
        Assert.IsTrue(game.CheckGameEnded());
        Assert.IsNull(game.GetWinner());
    }

    [TestMethod]
    public void ShouldRefusePowerAttackWithoutConsumingTurnWhenEnergyIsTooLow()
    {
        var game = CreateGame(out var host, out _);
        game.StartGame();

        // Énergie de départ = 2 : une première attaque puissante passe (pleine vie = réussite garantie)
        Assert.AreNotEqual(-1, game.ApplyAction(GameAction.ATTACK_PUISSANCE, out _));
        // Tour du client, il recharge
        game.ApplyAction(GameAction.RECHARGE, out _);
        // L'hôte n'a plus que 0 énergie : refus, et c'est toujours son tour
        Assert.AreEqual(-1, game.ApplyAction(GameAction.ATTACK_PUISSANCE, out _));
        Assert.AreSame(host, game.CurrentRobot);
    }

    [TestMethod]
    public void ShouldReturnZeroDamageWhenTargetDodged()
    {
        var game = CreateGame(out _, out var player);
        game.StartGame();

        game.ApplyAction(GameAction.RECHARGE, out _);                 // hôte
        game.ApplyAction(GameAction.DODGE, out bool dodged);           // client (pleine vie => réussite)
        int damage = game.ApplyAction(GameAction.ATTACK, out bool hit); // hôte

        Assert.IsTrue(dodged);
        Assert.IsTrue(hit);
        Assert.AreEqual(0, damage);
        Assert.AreEqual("100", player.GetStats(StatsType.HP));
    }
}
