using System;
using System.Collections.Generic;
using System.Text;

namespace RobotCombat.Domain.Communication
{
    /// <summary>
    /// Interface représentant un socket de communication.
    /// </summary>
    public interface ISocket
    {
        /// <summary>
        /// Démarrer la connexion
        /// </summary>
        Task Start();
        /// <summary>
        /// Envoyer un message via le socket
        /// </summary>
        public Task Send(string message);
        /// <summary>
        /// Recevoir un message via le socket
        /// </summary>
        public Task<string?> Receive();
        /// <summary>
        /// Fermer la connexion
        /// </summary>
        public void Exit();
        /// <summary>
        /// Vérifier si le socket est connecté
        /// </summary>
        public bool IsConnected();
    }
}
