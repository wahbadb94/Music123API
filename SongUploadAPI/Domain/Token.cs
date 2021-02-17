namespace SongUploadAPI.Domain
{
    public readonly struct Token
    {
        public Token(string value) => Value = value;

        public string Value { get; }
    }
}
