using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SongUploadAPI.Hubs
{
    public class JobUpdateHub : Hub
    {
        public Task AddNewUser(string newUserName)
        {
            return Clients.All.SendAsync("NewUserConnected", newUserName);
        }

        public Task ListenToJob(string jobId)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, jobId);
        }
    }
}
