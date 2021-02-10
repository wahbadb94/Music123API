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
using Microsoft.Extensions.Options;
using SongUploadAPI.Extensions;
using SongUploadAPI.Options;

namespace SongUploadAPI.Hubs
{
    [Authorize]
    public class JobUpdateHub : Hub
    {
        public ConcurrentDictionary<string, string> UserConnectionIds { get; }

        public JobUpdateHub()
        {
            UserConnectionIds = new ConcurrentDictionary<string, string>();
        }

        public override Task OnConnectedAsync()
        {
            var userId = Context.GetUserId();
            if (userId == string.Empty) return base.OnConnectedAsync();

            // add userId to group their own unique group, and
            // associate the connectionId with the userId in dictionary
            Groups.AddToGroupAsync(Context.ConnectionId, userId);
            UserConnectionIds.TryAdd(userId, Context.ConnectionId);

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.GetUserId();
            if (userId == string.Empty) return base.OnConnectedAsync();

            Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            UserConnectionIds.TryRemove(userId, out _);
            
            return base.OnDisconnectedAsync(exception);
        }
    }
}
