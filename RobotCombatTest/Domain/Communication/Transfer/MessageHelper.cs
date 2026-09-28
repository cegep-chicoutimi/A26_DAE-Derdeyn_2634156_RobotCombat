namespace RobotCombatTest;

using RobotCombat.Domain.Communication.Transfer;
using RobotCombat.Domain.Game;

[TestClass]
public class MessageHelperTest
{
    [TestMethod]
    public void ShouldBuildActionAttackMessage()
    {
        var expected = """
            {
              "messageType": 9,
              "action": 0,
              "data": "Test data",
              "status": 3
            }
            """;
        var actual = MessageHelper.BuildMessage(MessageType.ACTION, GameAction.ATTACK, GameStatus.PLAYING, "Test data");

        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void ShouldParseActionAttackMessage()
    {
        var expected = new Message
        {
            MessageType = MessageType.ACTION,
            Action = GameAction.ATTACK,
            Data = "Test data",
            Status = GameStatus.PLAYING
        };
        var actual = MessageHelper.ParseMessage("""
            {
              "messageType": 9,
              "action": 0,
              "data": "Test data",
              "status": 3
            }
            """);

        Assert.AreEqual(expected, actual);
    }
}