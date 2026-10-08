using RobotCombat.Domain.Communication.Transfer;
using RobotCombat.Domain.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace RobotCombat.Domain
{
    /// <summary>
    /// Interface représentant la vue du jeu, permettant d'interagir avec le joueur.
    /// </summary>
    public interface IGameView
    {
        /// <summary>
        /// Demander si le joueur souhaite rejoindre ou créer une partie
        /// </summary>
        public string AskPlayerType();

        /// <summary>
        /// Demander la configuration de l'hôte (IP et port)
        /// </summary>
        public string[] AskPlayerHostInformations();

        /// <summary>
        /// Afficher un message à l'utilisateur
        /// </summary>
        public void ShowMessage(string message);
        /// <summary>
        /// Afficher le gagnant de la partie (null = aucun gagnant, fin par fuite)
        /// </summary>
        public void ShowWinner(Robot? robot);
        /// <summary>
        /// Demander la configuration du robot du joueur
        /// </summary>
        public RobotConfig AskPlayerConfig();

        /// <summary>
        /// Demander l'action du robot à effectuer pour le tour en cours
        /// </summary>
        public GameAction AskPlayerAction();
        /// <summary>
        /// Afficher les statistiques de la partie en cours
        /// </summary>
        public void DisplayFight(Robot localRobot, Robot remoteRobot);

        /// <summary>
        /// Demander au joueur s'il souhaite rejouer
        /// </summary>
        public Boolean AskPlayerReplay();

        /// <summary>
        /// Demander le port d'exécution de l'application de l'hôte
        /// </summary>
        public int AskHostPortInformation();
    }
}
