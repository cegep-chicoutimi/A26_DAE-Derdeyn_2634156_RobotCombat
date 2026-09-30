namespace RobotCombatTest;

using RobotCombat.Domain.Communication.Transfer;
using RobotCombat.Domain.Game;

[TestClass]
public class MessageHelperTest
{
    [TestMethod]
    public void ShouldBuildActionAttackMessage()
    {
        var expected = "{\"type\":8,\"action\":0,\"status\":3,\"data\":\"Test data\"}";

        var actual = MessageHelper.BuildMessage(MessageType.PLAYER_ACTION, GameAction.ATTACK, GameStatus.PLAYING, "Test data");

        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void ShouldParseActionAttackMessage()
    {
        var expected = new Message
        {
            Type = MessageType.PLAYER_ACTION,
            Action = GameAction.ATTACK,
            Data = "Test data",
            Status = GameStatus.PLAYING
        };
        Console.WriteLine(MessageType.PLAYER_ACTION);
        var actual = MessageHelper.ParseMessage("""
        {
          "type": 8,
          "action": 0,
          "data": "Test data",
          "status": 3
        }
        """);

        Assert.AreEqual(expected, actual);
    }

}