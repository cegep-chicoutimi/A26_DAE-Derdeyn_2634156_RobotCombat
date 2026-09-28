using RobotCombat.Domain.Communication.Transfer;
using RobotCombat.Domain.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace RobotCombat.Domain
{
    /**
     * Interface représentant une commande pouvant être exécutée en réponse à un message reçu.
     */
    public interface ICommand
    {
        /**
         * Gère le message reçu en exécutant la commande correspondante.
         * @param message Le message à traiter.
         */
        public void Handle(Message message);
    }
}
