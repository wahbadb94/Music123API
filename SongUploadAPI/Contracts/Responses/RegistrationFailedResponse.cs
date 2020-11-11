using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SongUploadAPI.Contracts.Responses
{
    public class RegistrationFailedResponse
    {
        public IEnumerable<string> Errors { get; set; }
    }
}
