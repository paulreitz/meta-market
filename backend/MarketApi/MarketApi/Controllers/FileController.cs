using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MarketApi.Providers;

namespace MarketApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : Controller
    {
        private readonly IStorageProvider _storageProvider;
        private readonly string _storageRoot;

        public FileController(IStorageProvider storageProvider, IConfiguration configuration)
        {
            _storageProvider = storageProvider;
            _storageRoot = configuration.GetValue<string>("LocalStoragePath") ?? "Uploads";
        }

        [HttpGet("{*relativePath}")]
        public async Task<IActionResult> GetFile(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return BadRequest("File path is required.");
            }

            var cleanRelativePath = relativePath.TrimStart('/');
            var fullPath = Path.Combine(_storageRoot, cleanRelativePath);

            if (!await _storageProvider.FileExistsAsync(cleanRelativePath))
            {
                return NotFound("File not found.");
            }

            var extension = Path.GetExtension(fullPath).ToLowerInvariant();
            var contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };

            var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            return File(fileStream, contentType, enableRangeProcessing: true);
        }

    }
}
