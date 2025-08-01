using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MarketApi.Models;
using MarketApi.Services;
using Nethereum.Util;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using System.IO;

namespace MarketApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _environment;

        public UserController(AppDbContext context, IConfiguration configuration, IUserService userService, IWebHostEnvironment environment)
        {
            _context = context;
            _config = configuration;
            _userService = userService;
            _environment = environment;
        }

        [HttpGet("get/{walletAddress}")]
        public async Task<ActionResult<User>> GetUser(string walletAddress)
        {
            if (string.IsNullOrEmpty(walletAddress))
            {
                return BadRequest("Wallet address is required.");
            }

            if (!AddressUtil.Current.IsValidEthereumAddressHexFormat(walletAddress))
            {
                return BadRequest("Invalid Ethereum address.");
            }

            walletAddress = walletAddress.ToLower();

            var user = await _context.Users.FindAsync(walletAddress);
            if(user == null)
            {
                return NotFound("user not found.");
            }

            return Ok(_userService.GetUserResponse(user));
        }

        [HttpPut("username")]
        [Authorize]
        public async Task<ActionResult<User>> UpdateUsername([FromBody] UpdateUsernameRequest request)
        {
            var walletAddress = User.FindFirst(ClaimTypes.NameIdentifier)?.Value?.ToLower();
            if (string.IsNullOrEmpty(walletAddress))
            {
                return Unauthorized("User not authenticated.");
            }

            var user = await _context.Users.FindAsync(walletAddress);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (string.IsNullOrEmpty(request.Username) || request.Username.Trim().Length == 0)
            {
                return BadRequest("Username is required.");
            }

            if (request.Username.Length > 50)
            {
                return BadRequest("Username cannot exceed 50 characters.");
            }

            user.Username = request.Username.Trim();
            await _context.SaveChangesAsync();

            return Ok(_userService.GetUserResponse(user));
        }

        [HttpPut("bio")]
        [Authorize]
        public async Task<ActionResult<User>> UpdateUserBio([FromBody] UpdateUserBioRequest request)
        {
            var walletAddress = User.FindFirst(ClaimTypes.NameIdentifier)?.Value?.ToLower();
            if (string.IsNullOrEmpty(walletAddress))
            {
                return Unauthorized("User not authenticated.");
            }

            var user = await _context.Users.FindAsync(walletAddress);
            if (user == null)
            {
                return BadRequest("user not found.");
            }

            user.Bio = request.UserBio.Trim();
            await _context.SaveChangesAsync();

            return Ok(_userService.GetUserResponse(user));
        }

        [HttpPut("website")]
        [Authorize]
        public async Task<ActionResult<User>> UpdateUserWebsite([FromBody] UpdateUserWebsiteRequest request)
        {
            var walletAddress = User.FindFirst(ClaimTypes.NameIdentifier)?.Value?.ToLower();
            if (string.IsNullOrEmpty(walletAddress))
            {
                return Unauthorized("User not authenticated.");
            }

            var user = await _context.Users.FindAsync(walletAddress);
            if (user == null)
            {
                return BadRequest("user not found.");
            }

            user.Website = request.UserWebsite;
            await _context.SaveChangesAsync();

            return Ok(_userService.GetUserResponse(user));
        }

        [HttpPost("banner")]
        [Authorize]
        public async Task<ActionResult<User>> UpdateBanner(IFormFile file)
        {
            var walletAddress = User.FindFirst(ClaimTypes.NameIdentifier)?.Value?.ToLower();
            if (string.IsNullOrEmpty(walletAddress))
            {
                return Unauthorized("User not authenticated.");
            }

            var user = await _context.Users.FindAsync(walletAddress);
            if (user == null)
            {
                return NotFound("user not found.");
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest("Invalid file type. Only JPG, JPEG, PNG, and GIF are allowed.");
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest("File too large, Max 5MB");
            }

            var userFolder = Path.Combine(_environment.WebRootPath, "cdn", "images", "users", walletAddress); // GOOD GAWD, fix this for deployment!!!
            if (!Directory.Exists(userFolder))
            {
                Directory.CreateDirectory(userFolder);
            }

            if (!string.IsNullOrEmpty(user.BannerUrl) && user.BannerUrl.StartsWith("/images/users"))
            {
                var oldRelativePath = user.BannerUrl.TrimStart('/');
                var oldFilePath = Path.Combine(_environment.WebRootPath, "cdn", oldRelativePath);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            var cacheBust = Guid.NewGuid().ToString("N").Substring(0, 8);
            var filePath = Path.Combine(userFolder, $"banner{cacheBust}{extension}");
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            user.BannerUrl = $"/images/users/{walletAddress}/banner{cacheBust}{extension}";
            await _context.SaveChangesAsync();

            return Ok(_userService.GetUserResponse(user));
        }

        [HttpPost("pfp")]
        [Authorize]
        public async Task<ActionResult<User>> UpdatePfp(IFormFile file)
        {
            var walletAddress = User.FindFirst(ClaimTypes.NameIdentifier)?.Value?.ToLower();
            if (string.IsNullOrEmpty(walletAddress))
            {
                return Unauthorized("User not authenticated.");
            }

            var user = await _context.Users.FindAsync(walletAddress);
            if (user == null)
            {
                return NotFound("user not found.");
            }

            if (file == null ||file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest("Invalid file type. Only JPG, JPEG, PNG, and GIF are allowed.");
            }
            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest("File too large, Max 5MB");
            }

            var userFilder = Path.Combine(_environment.WebRootPath, "cdn", "images", "users", walletAddress); // GOOD GAWD, fix this for deployment!!!
            if (!Directory.Exists(userFilder))
            {
                Directory.CreateDirectory(userFilder);
            }

            if (!string.IsNullOrEmpty(user.PfpUrl) && user.PfpUrl.StartsWith("/images/users/"))
            {
                var oldRelativePath = user.PfpUrl.TrimStart('/');
                var oldFilePath = Path.Combine(_environment.WebRootPath, "cdn", oldRelativePath);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            var cacheBust = Guid.NewGuid().ToString("N").Substring(0, 8);

            var filePath = Path.Combine(userFilder, $"pfp{cacheBust}{extension}");
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            user.PfpUrl = $"/images/users/{walletAddress}/pfp{cacheBust}{extension}";
            await _context.SaveChangesAsync();

            return Ok(_userService.GetUserResponse(user));
        }
    }

    public class UpdateUsernameRequest
    {
        public string Username { get; set; }
    }

    public class UpdateUserBioRequest
    {
        public string UserBio { get; set; }
    }

    public class UpdateUserWebsiteRequest
    {
        public string UserWebsite { get; set; }
    }
}
