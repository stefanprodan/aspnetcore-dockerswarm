using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LogWatcher
{
    public class LogHub : Hub
    {
        public void Send(string timestamp, string message)
        {
            // Call the addNewMessageToPage method to update clients.
            Clients.All.OnLog(timestamp, message);
        }
    }
}
