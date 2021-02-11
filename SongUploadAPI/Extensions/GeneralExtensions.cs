using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Net.Http.Headers;

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

        public static bool HasFileContent(this ContentDispositionHeaderValue contentDisposition)
        {
            // Content-Disposition: form-data; name="file"; filename="Misc 002.jpg"
            return contentDisposition != null
                   && contentDisposition.DispositionType.Equals("form-data")
                   && (!string.IsNullOrEmpty(contentDisposition.FileName.Value)
                       || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
        }

        public static bool HasFormDataContent(this ContentDispositionHeaderValue contentDisposition)
        {
            // Content-Disposition: form-data; name="key";
            return contentDisposition != null
                   && contentDisposition.DispositionType.Equals("form-data")
                   && string.IsNullOrEmpty(contentDisposition.FileName.Value)
                   && string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);
        }

        public static bool IsMultiPartContentType(this HttpRequest request)
        {
            var contentType = request.ContentType;

            return !string.IsNullOrEmpty(contentType)
                   && contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
