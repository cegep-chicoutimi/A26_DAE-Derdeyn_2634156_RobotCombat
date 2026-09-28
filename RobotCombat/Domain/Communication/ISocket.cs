using System;
using System.Collections.Generic;
using System.Text;

namespace RobotCombat.Domain.Communication
{
    /**
     * Interface représentant un socket de communication.
     */
    public interface ISocket
    {
        /**
         * Démarrer la connexion
         */
        Task Start();
        /**
         * Envoyer un message via le socket
         */
        public Task Send(string message);
        /**
         * Recevoir un message via le socket
         */
        public Task<string?> Receive();
        /**
         * Fermer la connexion
         */
        public void Exit();
        /**
         * Vérifier si le socket est connecté
         */
        public bool IsConnected();
    }
}
