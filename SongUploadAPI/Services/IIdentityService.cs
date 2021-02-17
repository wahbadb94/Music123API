using SongUploadAPI.Domain;
using System.Threading.Tasks;

namespace SongUploadAPI.Services
{
    public interface IIdentityService
    {
        Task<Result<Token>> RegisterAsync(string email, string password);
        Task<Result<Token>> LoginAsync(string email, string password);
    }
}
