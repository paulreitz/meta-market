using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nethereum.Signer;
using Nethereum.Util;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MarketApi.Models;
using MarketApi.Services;

namespace MarketApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IUserService _userService;
        private readonly JwtSettings _jwtSettings;

        public AuthController(AppDbContext context, IConfiguration config, IUserService userService, IOptions<JwtSettings> JwtSettingsOptions)
        {
            _context = context;
            _config = config;
            _userService = userService;
            _jwtSettings = JwtSettingsOptions.Value ?? throw new InvalidOperationException("JwtSettings could not be resolved from configuration.");
        }

        [HttpGet("nonce/{walletAddress}")]
        public async Task<ActionResult<string>> GetNonce(string walletAddress)
        {
            if (!AddressUtil.Current.IsValidEthereumAddressHexFormat(walletAddress))
            {
                return BadRequest("Invalid Ethereum address.");
            }

            walletAddress = walletAddress.ToLower();

            var user = await _context.Users.FindAsync(walletAddress);
            if (user == null)
            {
                user = new User
                {
                    WalletAddress = walletAddress,
                    Username = walletAddress,
                    AdultContentEnabled = false,
                    Joined = DateTime.UtcNow
                };
                _context.Users.Add(user);
            }
            user.Nonce = Guid.NewGuid().ToString();
            user.NonceExpiration = DateTime.UtcNow.AddMinutes(5);
            await _context.SaveChangesAsync();

            return Ok(user.Nonce);
        }

        [HttpPost("verify")]
        public async Task<ActionResult<object>> VerifySignature([FromBody] VerifyRequest request)
        {
            if (!AddressUtil.Current.IsValidEthereumAddressHexFormat(request.WalletAddress))
            {
                return BadRequest("Invalid Ethereum address.");
            }

            var walletAddress = request.WalletAddress.ToLower();
            var user = await _context.Users.FindAsync(walletAddress);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (user.Nonce != request.Nonce || user.NonceExpiration < DateTime.UtcNow)
            {
                return BadRequest("Invalid or expired nonce.");
            }

            var message = $"Login to Marketplace with nonce: {request.Nonce}";
            var signer = new EthereumMessageSigner();
            var recoveredAddress = signer.EncodeUTF8AndEcRecover(message, request.Signature);

            if (recoveredAddress.ToLower() != walletAddress)
            {
                return Unauthorized("Signature verification failed.");
            }

            user.Nonce = null;
            user.NonceExpiration = null;
            await _context.SaveChangesAsync();

            var key = Encoding.ASCII.GetBytes(_config.GetValue<string>("TokenKey"));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.WalletAddress),
                    new Claim(ClaimTypes.Name, user.Username)
                }),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            return Ok(new
            {
                Message = "Authentication successfull.",
                User = _userService.GetUserResponse(user),
                Token = jwtToken
            });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<object>> GetCurrentUser()
        {
            var walletAddress = User.FindFirst(ClaimTypes.NameIdentifier)?.Value?.ToLower();
            if (string.IsNullOrEmpty(walletAddress))
            {
                return Unauthorized("No valid token provided.");
            }

            var user = await _context.Users.FindAsync(walletAddress);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(new
            {
                User = _userService.GetUserResponse(user),
            });
        }
    }

    public class VerifyRequest
    {
        public string WalletAddress { get; set; }
        public string Nonce { get; set; }
        public string Signature { get; set; }
    }
}
