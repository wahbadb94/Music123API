namespace SongUploadAPI.Contracts.Responses
{
    public record SongSuccessResponse(string Id,
        string Name,
        string Artist,
        string Key,
        int Bpm,
        string StreamingUrl);
}