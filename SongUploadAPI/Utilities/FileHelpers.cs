using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SongUploadAPI.Domain;

namespace SongUploadAPI.Utilities
{
    public static class FileHelpers
    {
        private static readonly string[] PermittedExtensions = {".wav"};
        private static readonly Dictionary<string, List<byte[]>> FileSignatures = new Dictionary<string, List<byte[]>>
        {
            {".wav", new List<byte[]> { new byte[] { 0x52, 0x49, 0x46, 0x46 } } },
        };

        public static async Task<Result<byte[]>> TryProcessFileAsync(Stream sectionBody, string fileName, long fileSizeLimit)
        {
            if (sectionBody == null) return new Error("The file stream does not exist");

            await using var memoryStream = new MemoryStream();
            await sectionBody.CopyToAsync(memoryStream);

            if (memoryStream.Length == 0) return new Error("The file is empty");

            if (memoryStream.Length > fileSizeLimit)
            {
                var megabyteSizeLimit = fileSizeLimit / 1048576;
                return new Error($"The file exceeds {megabyteSizeLimit:N1} MB.");
            }

            if (IsValidExtension(memoryStream, fileName) == false)
            {
                return new Error("This file type isn't permitted or the signature doesn't match the extension.");
            }

            return memoryStream.ToArray();
        }

        private static bool IsValidExtension(Stream dataStream, string fileName)
        {
            // check file name
            if (string.IsNullOrEmpty(fileName)) return false;

            // check file extenstion
            var extIndexOf = fileName.LastIndexOf('.');
            var ext = fileName.Substring(extIndexOf).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || (PermittedExtensions.Contains(ext) == false)) return false;

            //check file signature
            dataStream.Position = 0;
            var reader = new BinaryReader(dataStream);
            var signatures = FileSignatures[ext];
            var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));
            var result = signatures.Any(signature => headerBytes.Take(signature.Length).SequenceEqual(signature));
            return result;
        }
    }

}
