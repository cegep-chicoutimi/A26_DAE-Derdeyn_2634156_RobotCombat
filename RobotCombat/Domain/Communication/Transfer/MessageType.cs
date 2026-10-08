    using System;
    using System.Collections.Generic;
    using System.Text;

    namespace RobotCombat.Domain.Communication.Transfer
    {
    /// <summary>
    /// Enumération représentant les différents types de messages échangés entre le client et le serveur.
    /// </summary>
    public enum MessageType
    {
        PLAYER_JOIN,

        WELCOME,

        SERVER_BUSY,

        ROBOT_CONFIG,

        ROBOT_CONFIG_OK,

        ERROR,

        GAME_START,

        TURN,

        PLAYER_ACTION,

        PLAYER_RESULT,

        GAME_END,

        PLAYER_REPLAY,

        QUIT
    }
}
