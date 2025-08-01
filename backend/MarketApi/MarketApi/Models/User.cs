using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace MarketApi.Models
{
    public class User
    {
        [Key]
        public string WalletAddress { get; set; }
        public string Username { get; set; }
        public string PfpUrl { get; set; } = "/images/defaults/pfp.png";
        public string BannerUrl { get; set; } = "/images/defaults/banner.png";
        public string Bio { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public bool AgeVerified { get; set; } = false;
        public bool AdultContentEnabled { get; set; } = false;
        public DateTime Joined { get; set; } = DateTime.UtcNow;
        public string? Nonce { get; set; }
        public DateTime? NonceExpiration { get; set; }
    }
}
