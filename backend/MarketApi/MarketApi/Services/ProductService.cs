using Microsoft.Extensions.Configuration;
using MarketApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketApi.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IFileStorageService _fileStorageService;

        public ProductService(AppDbContext context, IConfiguration config, IFileStorageService fileStorageService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        }

        public async Task<Product> CreateProductAsync(CreateProductRequest request, string walletAddress)
        {
            var user = await _context.Users.FindAsync(walletAddress.ToLower());
            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Currency = request.Currency,
                UserWalletAddress = walletAddress,
                User = user,
                CreatedAt = DateTime.UtcNow,
                Images = new List<string>(),
                Files = new List<ProductFile>()
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            if (request.Images.Any())
            {
                product.Images = await _fileStorageService.SaveProductImagesAsync(product.Id, request.Images);
            }

            if (request.Files.Any())
            {
                product.Files = await _fileStorageService.SaveProductFilesAsync(product.Id, request.Files);
            }

            await _context.SaveChangesAsync();
            return product;
        }

        public Product GetProductResponse(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }
            var cdnBaseUrl = _config.GetValue<string>("CdnBaseUrl") ?? "";
            var fullImageUrls = product.Images
                .Select(image => $"{cdnBaseUrl.TrimEnd('/')}/{image.TrimStart('/')}")
                .ToList();

            var fileResponses = product.Files
                .Select(file => GetProductFileResponse(file))
                .ToList();

            return new Product
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Images = fullImageUrls,
                UserWalletAddress = product.UserWalletAddress,
                User = product.User, // Include for now, but consider a watered down version with only necessary fields
                Files = fileResponses,
                Price = product.Price,
                Currency = product.Currency,
                CreatedAt = product.CreatedAt
            };
        }

        /*
         * For now, I'm placing the ProductFile methods in this class for simplicity.
         * If things get out of hand, these methods can be moved to a separate Service.
         * Place ALL ProductFile related mehtods below this comment to make any future refactoring easier.
         */

        public ProductFile GetProductFileResponse(ProductFile productFile)
        {
            if (productFile == null)
            {
                throw new ArgumentNullException(nameof(productFile));
            }

            return new ProductFile
            {
                Id = productFile.Id,
                FileName = productFile.FileName,
                FilePath = string.Empty, // Do NOT send the file path to the client
                FileType = productFile.FileType,
                FileSize = productFile.FileSize,
                UploadedAt = productFile.UploadedAt,
                ProductId = productFile.ProductId,
            };
        }
    }
}
