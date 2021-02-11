using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SongUploadAPI.Domain;
using SongUploadAPI.Hubs;

namespace SongUploadAPI.Services
{
    public class JobNotificationService : IJobNotificationService
    {
        private readonly JobUpdateHub _jobUpdateHub;

        public JobNotificationService(JobUpdateHub jobUpdateHub)
        {
            _jobUpdateHub = jobUpdateHub;
        }

        public async Task NotifyUserJobStateChange(string userId, JobState newJobState)
        {
            switch (newJobState)
            {
                case JobState.Submitting:
                case JobState.Uploading:
                case JobState.Encoding:
                case JobState.Finalizing:
                case JobState.Finished:
                    await _jobUpdateHub.Clients.Group(userId).SendAsync(
                        "jobStateChange", 
                        newJobState.ToString().ToLower());
                    break;
                case JobState.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newJobState),
                        newJobState,
                        $"{newJobState} is not a valid variant of the JobState enum");
            }
        }

        public void NotifyUserUploadPercentageChange(string userId, double percentage)
        {
            _jobUpdateHub.Clients.Groups(userId).SendAsync("uploadPercentageChange", percentage);
        }
    }
}
