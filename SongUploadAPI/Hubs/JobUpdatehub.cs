using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SongUploadAPI.Hubs
{
    [Authorize]
    public class JobUpdateHub : Hub
    {
        public ConcurrentDictionary<string, string> UserConnectionIds { get; private set; }

        public JobUpdateHub()
        {
            UserConnectionIds = new ConcurrentDictionary<string, string>();
        }

        public override Task OnConnectedAsync()
        {
            var userName = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            Groups.AddToGroupAsync(Context.ConnectionId, userName);
            Console.WriteLine($"Added: {userName}");

            // add this connectionId to list of connectionIds associated with this user
            UserConnectionIds.TryAdd(userName, Context.ConnectionId);

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var userName = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            Groups.RemoveFromGroupAsync(Context.ConnectionId, userName);
            Console.WriteLine($"Disconnected: {userName}");
            
            UserConnectionIds.TryRemove(userName, out _);
            
            return base.OnDisconnectedAsync(exception);
        }
    }
}
