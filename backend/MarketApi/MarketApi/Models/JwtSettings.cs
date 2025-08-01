namespace MarketApi.Models
{
    public class JwtSettings
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpirationMinutes { get; set; } = 60;
        public int RefreshTokenExpirationDays { get; set; } = 30;
    }
}
