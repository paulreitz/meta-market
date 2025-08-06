using MarketApi.Models;
using MarketApi.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<List<string>> SaveProductImagesAsync(int productId, List<IFormFile> images)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var imagePaths = new List<string>();

            if (images == null || !images.Any())
            {
                return imagePaths;
            }

            foreach (var image in images)
            {
                if (image == null || image.Length == 0)
                {
                    throw new ArgumentException("One or more images are empty.");
                }
                if (image.Length > 5 * 1024 * 1024)
                {
                    throw new ArgumentException("Image too large, Max 5MB");
                }
                var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    throw new ArgumentException("Invalid image type. Only JPG, JPEG, PNG, and GIF are allowed.");
                }
                
                var safeFileName = Path.GetFileName(image.FileName).Replace("..", "").Replace("/", "").Replace("\\", "");
                var filePath = Path.Combine("images", "products", productId.ToString(), safeFileName)
                    .Replace("\\", "/");

                // Include? Don't include? Let the user replace files?
                //if (await _storageProvider.FileExistsAsync(filePath))
                //{
                //    throw new ArgumentException($"File {safeFileName} already exists for product {productId}. Please rename the file and try again.");
                //}

                await _storageProvider.SaveFileAsync(filePath, image.OpenReadStream());
                imagePaths.Add($"/{filePath}");
            }
            return imagePaths;
        }

        public async Task<List<ProductFile>> SaveProductFilesAsync(int productId, List<IFormFile> files)
        {
            var productFiles = new List<ProductFile>();

            if (files == null || !files.Any())
            {
                return productFiles;
            }

            foreach (var file in files)
            {
                if (file == null || file.Length == 0)
                {
                    if (file == null || file.Length == 0)
                    {
                        throw new ArgumentException("One or more files are empty.");
                    }
                    if (file.Length > 50 * 1024 * 1024)
                    {
                        throw new ArgumentException("File too large, Max 50MB");
                    }
                    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                    var safeFileName = Path.GetFileName(file.FileName).Replace("..", "").Replace("/", "").Replace("\\", "");
                    var filePath = Path.Combine("files", "products", productId.ToString(), safeFileName)
                        .Replace("\\", "/");
                    await _storageProvider.SaveFileAsync(filePath, file.OpenReadStream());
                    productFiles.Add(new ProductFile
                    {
                        ProductId = productId,
                        FileName = file.FileName,
                        FilePath = $"/{filePath}",
                        FileType = file.ContentType,
                        FileSize = file.Length,
                        UploadedAt = DateTime.UtcNow
                    });
                }
            }
            return productFiles;
        }
    }
}
