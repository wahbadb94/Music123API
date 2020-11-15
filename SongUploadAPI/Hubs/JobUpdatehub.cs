using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace SongUploadAPI.Hubs
{
    [Authorize]
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

        public override Task OnConnectedAsync()
        {
            var userName = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine(userName);
            return base.OnConnectedAsync();
        }
    }
}
