using RobotCombat.Domain.Communication.Transfer;
using RobotCombat.Domain.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace RobotCombat.Domain
{
    /**
     * Interface représentant la vue du jeu, permettant d'interagir avec le joueur.
     */
    public interface IGameView
    {
        /**
         * Demander si le joueur souhaite rejoindre ou créer une partie
         */
        public string AskPlayerType();

        /**
         * Demander la configuration de l'hôte (IP et port)
         */
        public string[] AskPlayerHostInformations();

        /**
         * Afficher un message à l'utilisateur
         */
        public void ShowMessage(string message);
        /**
         * Afficher le gagnant de la partie
         */
        public void ShowWinner(Robot robot);
        /**
         * Demander la configuration du robot du joueur
         */
        public RobotConfig AskPlayerConfig();

        /**
         * Demander l'action du robot à effectuer pour le tour en cours
         */
        public GameAction AskPlayerAction();
        /**
         * Afficher les statistiques de la partie en cours
         */
        public void DisplayFight(Robot localRobot, Robot remoteRobot);

        /**
         * Demander au joueur s'il souhaite rejouer
         */
        public Boolean AskPlayerReplay();

        /**
         * Demander le port d'exécution de l'application de l'hôte
         */
        public int AskHostPortInformation();
    }
}
