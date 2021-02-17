namespace SongUploadAPI.Options
{
    public class JwtSettings
    {
        public string Secret { get; set; }
        public string UserIdClaimName { get; set; }
    }
}
