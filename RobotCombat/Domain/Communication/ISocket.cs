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
        Task Start();
        public Task Send(string message);
        public Task<string?> Receive();
        public void Exit();

        public bool IsConnected();
    }
}
