using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using _21BITV03_Nhom04_website_clothes.Data;
using Microsoft.AspNetCore.Authorization;

namespace _21BITV03_Nhom04_website_clothes.Controllers
{
    [Authorize]
    [Authorize(Roles = "Admin")]
    public class SubProductsController : Controller
    {
        private readonly WebsiteClothesContext _context;

        public SubProductsController(WebsiteClothesContext context)
        {
            _context = context;
        }

        // GET: SubProducts
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
      .Include(p => p.SubProducts)
          .ThenInclude(sp => sp.Color)
      .Include(p => p.SubProducts)
          .ThenInclude(sp => sp.Size)
      .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }
            ViewData["MainProductId"] = id;

            // Pass the product ID to the View for better context
            ViewBag.MainProductId = product.ProductId;

            return View(product.SubProducts.ToList());
        }

        // GET: SubProducts/Create
        public IActionResult Create(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            ViewData["MainProductId"] = id;
            ViewData["ColorId"] = new SelectList(_context.ProductColors, "ColorId", "ColorName");
            ViewData["SizeId"] = new SelectList(_context.ProductSizes, "ProductSizeId", "SizeName");
            return View();
        }

        // POST: SubProducts/Create
        // POST: SubProducts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SubProductId,MainProductId,OriginalPrice,DiscountedPrice,ColorId,SizeId,Linkimage")] SubProduct subProduct, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                subProduct.CreationDate = DateTime.Now;

                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Path.GetFileName(imageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Product_img/", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    subProduct.Linkimage = Path.Combine("/Product_img/", fileName);
                }
                
                _context.Add(subProduct);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { id = subProduct.MainProductId });
            }

            ViewData["MainProductId"] = subProduct.MainProductId;
            ViewData["ColorId"] = new SelectList(_context.ProductColors, "ColorId", "ColorName", subProduct.ColorId);
            ViewData["SizeId"] = new SelectList(_context.ProductSizes, "ProductSizeId", "SizeName", subProduct.SizeId);
            return View(subProduct);
        }

        // GET: SubProducts/Edit/5
        // GET: SubProducts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subProduct = await _context.SubProducts.FindAsync(id);
            if (subProduct == null)
            {
                return NotFound();
            }
            ViewData["MainProductId"] = id;

            // Populate the dropdown lists
            ViewData["ColorId"] = new SelectList(_context.ProductColors, "ColorId", "ColorName", subProduct.ColorId);
            ViewData["SizeId"] = new SelectList(_context.ProductSizes, "ProductSizeId", "SizeName", subProduct.SizeId);

            return View(subProduct);
        }

        // POST: SubProducts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SubProductId,MainProductId,OriginalPrice,DiscountedPrice,ColorId,SizeId,CreationDate,Linkimage")] SubProduct subProduct, IFormFile imageFile)
        {
            if (id != subProduct.SubProductId)
            {
                return NotFound();
            }

            if (subProduct!=null || ModelState.IsValid)
            {
                try
                {
                    var existingSubProduct = await _context.SubProducts.AsNoTracking().FirstOrDefaultAsync(sp => sp.SubProductId == id);

                    if (existingSubProduct == null)
                    {
                        return NotFound();
                    }

                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var fileName = Path.GetFileName(imageFile.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Product_img/", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        subProduct.Linkimage = Path.Combine("/Product_img/", fileName);
                    }
                    else
                    {
                        subProduct.Linkimage = existingSubProduct.Linkimage;
                    }

                    _context.Update(subProduct);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index), new { id = subProduct.MainProductId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubProductExists(subProduct.SubProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewData["ColorId"] = new SelectList(_context.ProductColors, "ColorId", "ColorName", subProduct.ColorId);
            ViewData["SizeId"] = new SelectList(_context.ProductSizes, "ProductSizeId", "SizeName", subProduct.SizeId);
            return View(subProduct);
        }



        // GET: SubProducts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subProduct = await _context.SubProducts
                .Include(s => s.Color)
                .Include(s => s.MainProduct)
                .Include(s => s.Size)
                .FirstOrDefaultAsync(m => m.SubProductId == id);
            if (subProduct == null)
            {
                return NotFound();
            }

            return View(subProduct);
        }

        // POST: SubProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subProduct = await _context.SubProducts.FindAsync(id);
            if (subProduct != null)
            {
                _context.SubProducts.Remove(subProduct);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { id = subProduct.MainProductId });
        }

        private bool SubProductExists(int id)
        {
            return _context.SubProducts.Any(e => e.SubProductId == id);
        }
    }
}
