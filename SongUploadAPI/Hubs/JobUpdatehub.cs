using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SongUploadAPI.Hubs
{
    public class JobUpdateHub : Hub
    {
        public Task ListenToJob(string jobId)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, jobId);
        }

        public Task SendToAll(string username, string message)
        {
            return Clients.All.SendAsync("SendToAll", username, message);

        }

        public Task AddNewUser(string newUserName)
        {
            return Clients.All.SendAsync("NewUserConnected", newUserName);
        }

        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"New Connection: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine($"Disconnected: {Context.ConnectionId}");
            return base.OnDisconnectedAsync(exception);
        }
    }
}
