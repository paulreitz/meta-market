using MarketApi.Models;

namespace MarketApi.Services
{
    public interface IProductService
    {
        Task<Product> CreateProductAsync(CreateProductRequest request, string walletAddress);
        Product GetProductResponse(Product product);
        ProductFile GetProductFileResponse(ProductFile productFile);
    }
}
