using MarketApi.Models;

namespace MarketApi.Services
{
    public interface IProductService
    {
        Product GetProductResponse(Product product);
        ProductFile GetProductFileResponse(ProductFile productFile);
    }
}
