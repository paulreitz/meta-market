using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MarketApi.Models;
using MarketApi.Services;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace MarketApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IProductService _productService;

        public ProductController(AppDbContext context, IConfiguration configuration, IProductService productService)
        {
            _context = context;
            _config = configuration;
            _productService = productService;
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<ActionResult<Product>> CreateProductAsync([FromForm] CreateProductRequest request)
        {
            var walletAddress = User.FindFirst(ClaimTypes.NameIdentifier)?.Value?.ToLower();
            if (string.IsNullOrEmpty(walletAddress))
            {
                return Unauthorized("User not authenticated.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdProduct = await _productService.CreateProductAsync(request, walletAddress);
                return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest($"Invalid request: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                return BadRequest($"Error creating product: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            if (id <= 0)
            {
                return BadRequest("invalid product ID.");
            }

            var product = await _context.Products
                .Include(p => p.User)
                .Include(p => p.Files)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound("Product not found.");
            }

            return Ok(_productService.GetProductResponse(product));
        }
    }
}
