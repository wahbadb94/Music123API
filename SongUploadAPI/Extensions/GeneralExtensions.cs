using System.Linq;
using Microsoft.AspNetCore.Http;

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
    }
}
