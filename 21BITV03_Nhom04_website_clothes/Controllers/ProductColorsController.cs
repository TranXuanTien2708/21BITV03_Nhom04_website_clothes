using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using _21BITV03_Nhom04_website_clothes.Data;
using Microsoft.AspNetCore.Authorization;

namespace _21BITV03_Nhom04_website_clothes.Controllers
{
    [Authorize]
    [Authorize(Roles = "Admin")]
    public class ProductColorsController : Controller
    {
        private readonly WebsiteClothesContext _context;

        public ProductColorsController(WebsiteClothesContext context)
        {
            _context = context;
        }

        // GET: ProductColors
        public async Task<IActionResult> Index()
        {
            return View(await _context.ProductColors.ToListAsync());
        }

        // GET: ProductColors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productColor = await _context.ProductColors
                .FirstOrDefaultAsync(m => m.ColorId == id);
            if (productColor == null)
            {
                return NotFound();
            }

            return View(productColor);
        }

        // GET: ProductColors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ProductColors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ColorId,ColorName,Description")] ProductColor productColor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(productColor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(productColor);
        }

        // GET: ProductColors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productColor = await _context.ProductColors.FindAsync(id);
            if (productColor == null)
            {
                return NotFound();
            }
            return View(productColor);
        }

        // POST: ProductColors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ColorId,ColorName,Description")] ProductColor productColor)
        {
            if (id != productColor.ColorId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(productColor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductColorExists(productColor.ColorId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(productColor);
        }

        // GET: ProductColors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productColor = await _context.ProductColors
                .FirstOrDefaultAsync(m => m.ColorId == id);
            if (productColor == null)
            {
                return NotFound();
            }

            return View(productColor);
        }

        // POST: ProductColors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productColor = await _context.ProductColors.FindAsync(id);
            if (productColor != null)
            {
                _context.ProductColors.Remove(productColor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductColorExists(int id)
        {
            return _context.ProductColors.Any(e => e.ColorId == id);
        }
    }
}
