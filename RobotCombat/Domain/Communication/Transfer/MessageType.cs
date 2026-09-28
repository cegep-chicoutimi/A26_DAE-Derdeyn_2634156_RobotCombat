    using System;
    using System.Collections.Generic;
    using System.Text;

    namespace RobotCombat.Domain.Communication.Transfer
    {
    /**
     * Enumération représentant les différents types de messages échangés entre le client et le serveur.
     */
    public enum MessageType
    {
        CREATE,
        JOIN,
        WELCOME,
        SERVER_BUSY,
        ROBOT,
        ROBOT_OK,
        ERROR,
        START,
        TURN,
        ACTION,
        RESULT,
        END,
        REPLAY,
        REPLAY_OK,
        QUIT,
        QUIT_OK
    }
}
