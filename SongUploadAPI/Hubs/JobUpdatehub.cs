using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace SongUploadAPI.Hubs
{
    [Authorize]
    public class JobUpdateHub : Hub
    {
        public Task ListenToJob(string jobId)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, jobId);
        }

        public override Task OnConnectedAsync()
        {
            var userName = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            Groups.AddToGroupAsync(Context.ConnectionId, userName);
            return base.OnConnectedAsync();
        }
    }
}
