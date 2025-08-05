using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace MarketApi.Providers
{
    public class LocalFileStorageProvider : IStorageProvider
    {
        private readonly IConfiguration _config;
        private readonly string _storageRoot;

        public LocalFileStorageProvider(IConfiguration config)
        {
            _config = config;
            _storageRoot = _config.GetValue<string>("LocalStoragePath") ?? "Uploads";
        }

        public async Task SaveFileAsync(string relativePath, Stream fileStream)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                throw new ArgumentNullException("Relative path cannot be null or empty.", nameof(relativePath));
            }

            if (fileStream == null)
            {
                throw new ArgumentNullException(nameof(fileStream), "File stream cannot be null.");
            }

            var cleanRelativePath = relativePath.TrimStart('/');
            var fullPath = Path.Combine(_storageRoot, cleanRelativePath);
            var directory = Path.GetDirectoryName(fullPath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var fileStreamOutput = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
            {
                await fileStream.CopyToAsync(fileStreamOutput);
            }
        }

        public Task<bool> FileExistsAsync(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                throw new ArgumentNullException("Relative path cannot be null or empty.", nameof(relativePath));
            }
            var cleanRelativePath = relativePath.TrimStart('/');
            var fullPath = Path.Combine(_storageRoot, cleanRelativePath);
            return Task.FromResult(File.Exists(fullPath));
        }

        public Task DeleteFileAsync(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                throw new ArgumentNullException("Relative path cannot be null or empty.", nameof(relativePath));
            }

            var cleanRelativePath = relativePath.TrimStart('/');
            var fullPath = Path.Combine(_storageRoot, cleanRelativePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
            return Task.CompletedTask;
        }
    }
}
