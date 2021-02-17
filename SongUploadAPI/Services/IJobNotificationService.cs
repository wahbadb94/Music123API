using System.Threading.Tasks;
using SongUploadAPI.Domain;

namespace SongUploadAPI.Services
{
    public interface IJobNotificationService
    {
        public Task NotifyUserJobStateChange(string userId, JobState newJobState);

        public void NotifyUserUploadPercentageChange(string userId, double percentage);
    }
}
