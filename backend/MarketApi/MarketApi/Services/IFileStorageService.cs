using MarketApi.Models;

namespace MarketApi.Services
{
    public interface IFileStorageService
    {
        Task<string> SetPfpAsync(string walletAddress, IFormFile file, string oldPfpPath = null);
        Task<string> SetBannerAsync(string walletAddress, IFormFile file, string oldBannerPath = null);
        Task<List<string>> SaveProductImagesAsync(int productId, List<IFormFile> images);
        Task<List<ProductFile>> SaveProductFilesAsync(int productId, List<IFormFile> files);
    }
}
