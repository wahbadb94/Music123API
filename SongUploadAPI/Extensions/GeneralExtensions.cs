using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace SongUploadAPI.Extensions
{
    public static class GeneralExtensions
    {
        public static string GetUserId(this HttpContext httpContext)
        {
            return httpContext.User == null
                ? string.Empty
                : httpContext.User.Claims.Single(claim => claim.Type == "id").Value;
        }

        public static string GetUserId(this HubCallerContext context)
        {
            return context.User == null
                ? string.Empty
                : context.User.Claims.Single(claim => claim.Type == "id").Value;
        }
    }
}
