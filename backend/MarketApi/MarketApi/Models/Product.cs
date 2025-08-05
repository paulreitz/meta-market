using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketApi.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Images { get; set; } = new List<string>();
        [Required]
        public string UserWalletAddress { get; set; }
        [ForeignKey("UserWalletAddress")]
        public User User { get; set; }
        public List<ProductFile> Files { get; set; } = new List<ProductFile>();
        public decimal Price { get; set; }
        public Currency Currency { get; set; } = Currency.ETH;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
