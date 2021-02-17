namespace SongUploadAPI.Contracts.Requests
{
    public class SongFormData
    {
        public string Name { get; set; }
        public string Artist { get; set; }
        public string Key { get; set; }
        public int Bpm { get; set; }
    }
}
