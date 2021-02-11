namespace SongUploadAPI.Domain
{
    public enum JobState
    {
        None,
        Submitting,
        Uploading,
        Encoding,
        Finalizing,
        Finished
    }
}
