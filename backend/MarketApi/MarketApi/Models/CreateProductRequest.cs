using System.ComponentModel.DataAnnotations;

namespace MarketApi.Models
{
    public class CreateProductRequest
    {
        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Product name must be between 3 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } = string.Empty;

        public List<IFormFile> Images { get; set; } = new List<IFormFile>();

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Currency is required.")]
        [EnumDataType(typeof(Currency), ErrorMessage = "Invalid currency type.")]
        public Currency Currency { get; set; } = Currency.ETH;

        public List<IFormFile> Files { get; set; } = new List<IFormFile>();
    }
}
