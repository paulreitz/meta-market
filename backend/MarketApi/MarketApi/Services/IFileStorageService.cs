namespace MarketApi.Services
{
    public interface IFileStorageService
    {
        Task<string> SetPfpAsync(string walletAddress, IFormFile file, string oldPfpPath = null);
        Task<string> SetBannerAsync(string walletAddress, IFormFile file, string oldBannerPath = null);
    }
}
