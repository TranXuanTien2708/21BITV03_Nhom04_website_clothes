using _21BITV03_Nhom04_website_clothes.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _21BITV03_Nhom04_website_clothes.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly WebsiteClothesContext _context;

        public CartController(WebsiteClothesContext context)
        {
            _context = context;
        }

        [HttpGet("GetCartCount")]
        public async Task<IActionResult> GetCartCount()
        {

            var username = HttpContext.User.Identity?.Name;
            if (username == null)
            {
                return Unauthorized();
            }

            var userId = await _context.UserInfos
                .Where(u => u.UserName == username)
                .Select(u => u.UserId)
                .FirstOrDefaultAsync();

            if (userId == null)
            {
                return Unauthorized();
            }

            var cart = await _context.Carts
                .Include(c => c.CartProductLists)
                .ThenInclude(cpl => cpl.SubProduct) // Ensure to include SubProduct
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return NotFound("Cart not found for the current user.");
            }

            // Count the number of unique SubProduct items in the cart
            var distinctSubProductCount = cart.CartProductLists
                .Select(cpl => cpl.SubProductId) // Select SubProductId
                .Distinct() // Ensure uniqueness
                .Count();

            return Ok(new { cartCount = distinctSubProductCount });
        }
    }
}
