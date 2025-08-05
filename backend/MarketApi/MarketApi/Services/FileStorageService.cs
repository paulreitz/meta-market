using MarketApi.Providers;

namespace MarketApi.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IStorageProvider _storageProvider;

        public FileStorageService(IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public async Task<string> SetPfpAsync(string walletAddress, IFormFile file, string oldFilePath = null)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                throw new ArgumentException("Invalid file type. Only JPG, JPEG, PNG, and GIF are allowed.");
            }
            if (file.Length > 5 * 1024 * 1024)
            {
                throw new ArgumentException("File too large, Max 5MB");
            }
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file uploaded.");
            }

            if (oldFilePath != null)
            {
                var oldRelativePath = oldFilePath.TrimStart('/');
                if (await _storageProvider.FileExistsAsync(oldRelativePath))
                {
                    await _storageProvider.DeleteFileAsync(oldRelativePath);
                }
            }
            
            var cacheBuster = Guid.NewGuid().ToString("N").Substring(0, 8);
            var filePath = Path.Combine("images", "users", walletAddress, $"pfp{cacheBuster}{extension}")
                .Replace("\\", "/");
            await _storageProvider.SaveFileAsync(filePath, file.OpenReadStream());
            return $"/{filePath}";
        }

        public async Task<string> SetBannerAsync(string walletAddress, IFormFile file, string oldFilePath = null)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                throw new ArgumentException("Invalid file type. Only JPG, JPEG, PNG, and GIF are allowed.");
            }
            if (file.Length > 5 * 1024 * 1024)
            {
                throw new ArgumentException("File too large, Max 5MB");
            }
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file uploaded.");
            }
            if (oldFilePath != null)
            {
                var oldRelativePath = oldFilePath.TrimStart('/');
                if (await _storageProvider.FileExistsAsync(oldRelativePath))
                {
                    await _storageProvider.DeleteFileAsync(oldRelativePath);
                }
            }
            var cacheBuster = Guid.NewGuid().ToString("N").Substring(0, 8);
            var filePath = Path.Combine("images", "users", walletAddress, $"banner{cacheBuster}{extension}")
                .Replace("\\", "/");
            await _storageProvider.SaveFileAsync(filePath, file.OpenReadStream());
            return $"/{filePath}";
        }
    }
}
