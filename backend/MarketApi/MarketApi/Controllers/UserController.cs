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
        private readonly IFileStorageService _fileStorageService;

        public UserController(AppDbContext context, IConfiguration configuration, IUserService userService, IFileStorageService fileStorage, IWebHostEnvironment environment)
        {
            _context = context;
            _config = configuration;
            _userService = userService;
            _fileStorageService = fileStorage;
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

            try
            {
                string? oldBannerPath = !string.IsNullOrEmpty(user.BannerUrl) && user.BannerUrl.StartsWith("/images/users/")
                    ? user.BannerUrl.TrimStart('/')
                    : null;

                var newBannerPath = await _fileStorageService.SetBannerAsync(walletAddress, file, oldBannerPath);

                user.BannerUrl = newBannerPath;
                await _context.SaveChangesAsync();
                return Ok(_userService.GetUserResponse(user));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while updating the banner.");
            }
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

            try
            {
                string? oldPfpPath = !string.IsNullOrEmpty(user.PfpUrl) && user.PfpUrl.StartsWith("/images/users/")
                    ? user.PfpUrl.TrimStart('/')
                    : null;

                var newPfpPath = await _fileStorageService.SetPfpAsync(walletAddress, file, oldPfpPath);

                user.PfpUrl = newPfpPath;
                await _context.SaveChangesAsync();
                return Ok(_userService.GetUserResponse(user));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while updating the profile picture.");
            }
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
