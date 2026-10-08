using RobotCombat.Domain.Communication.Transfer;
using RobotCombat.Domain.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace RobotCombat.Domain
{
    /// <summary>
    /// Interface représentant une commande pouvant être exécutée en réponse à un message reçu.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Gère le message reçu en exécutant la commande correspondante.
        /// </summary>
        /// <param name="message">Le message à traiter.</param>
        public void Handle(Message message);
    }
}
