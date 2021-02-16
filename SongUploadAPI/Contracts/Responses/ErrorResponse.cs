namespace SongUploadAPI.Contracts.Responses
{
    public readonly struct ErrorResponse
    {
        public string Message { get; }
        
        public ErrorResponse(string message) => Message = message;

    }
}
