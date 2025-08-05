using Microsoft.Extensions.Configuration;
using MarketApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketApi.Services
{
    public class ProductService : IProductService
    {
        private readonly IConfiguration _config;

        public ProductService(IConfiguration config)
        {
            _config = config;
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
                User = product.User, // Include user for now to link to profile and/or products page.
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
