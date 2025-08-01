using Microsoft.AspNetCore.Mvc;
using MarketApi.Models;

namespace MarketApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyController : Controller
    {
        [HttpGet("supported")]
        public IActionResult GetSupportedCurrencies() // Sync/Blocking for now - consider async later.
        {
            var currencies = Enum.GetValues(typeof(Currency))
                .Cast<Currency>()
                .Select(c => c.ToString())
                .ToList();
            return Ok(currencies);
        }
    }
}
