using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace SongUploadAPI.Utilities
{
    public static class RequestHelpers
    {
        public static Encoding GetEncoding(MultipartSection section)
        {
            var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out var mediaType);

            if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding)) return Encoding.UTF8;

            return mediaType.Encoding;
        }
    }
}
