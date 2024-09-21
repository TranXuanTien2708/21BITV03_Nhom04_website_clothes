using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using _21BITV03_Nhom04_website_clothes.Data;
using _21BITV03_Nhom04_website_clothes.Models;
using Microsoft.AspNetCore.Authorization;

namespace _21BITV03_Nhom04_website_clothes.Controllers
{
    [Authorize]
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly WebsiteClothesContext _context;

        public ProductsController(WebsiteClothesContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            return View(await _context.Products.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            var viewModel = new ProductViewAdminModel
            {
                AvailableProductTypes = _context.ProductTypes.ToList() // Fetch the available product types from the database
            };
            return View(viewModel);
        }


        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewAdminModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    ProductName = viewModel.ProductName,
                    Description = viewModel.Description,
                    DeleteStatus = false,
                    DeletionDate = null,
                    Status = true
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // Add selected ProductTypeLinks
                foreach (var productTypeId in viewModel.SelectedProductTypeIds)
                {
                    var productTypeLink = new ProductTypeLink
                    {
                        ProductId = product.ProductId,
                        ProductTypeId = productTypeId
                    };
                    _context.ProductTypeLinks.Add(productTypeLink);
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Re-populate AvailableProductTypes if the model state is invalid
            viewModel.AvailableProductTypes = _context.ProductTypes.ToList();
            return View(viewModel);
        }
        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Populate the view model
            var viewModel = new ProductViewAdminModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Description = product.Description,
                DeleteStatus = product.DeleteStatus,
                Status = product.Status,
                AvailableProductTypes = _context.ProductTypes.ToList(), // Load available product types
                SelectedProductTypeIds = _context.ProductTypeLinks
    .Where(pt => pt.ProductId == product.ProductId)
    .Select(pt => pt.ProductTypeId.HasValue ? pt.ProductTypeId.Value : 0) // Convert nullable to non-nullable
    .ToList()
            };

            return View(viewModel);
        }
        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductViewAdminModel viewModel)
        {
            if (id != viewModel.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var product = await _context.Products.FindAsync(id);
                    if (product == null)
                    {
                        return NotFound();
                    }

                    // Update product fields
                    product.ProductName = viewModel.ProductName;
                    product.Description = viewModel.Description;
                    product.Status = viewModel.Status;
                    product.DeleteStatus = viewModel.DeleteStatus;

                    // Automatically set DeletionDate if DeleteStatus is true
                    if (viewModel.DeleteStatus == true)
                    {
                        product.DeletionDate = DateTime.Now;
                    }
                    else
                    {
                        product.DeletionDate = null; // Clear the DeletionDate if DeleteStatus is false
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();

                    // Update ProductTypeLinks
                    var existingLinks = _context.ProductTypeLinks.Where(pt => pt.ProductId == id).ToList();
                    _context.ProductTypeLinks.RemoveRange(existingLinks);
                    await _context.SaveChangesAsync();

                    foreach (var productTypeId in viewModel.SelectedProductTypeIds)
                    {
                        var productTypeLink = new ProductTypeLink
                        {
                            ProductId = product.ProductId,
                            ProductTypeId = productTypeId
                        };
                        _context.ProductTypeLinks.Add(productTypeLink);
                    }

                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(viewModel.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Re-populate AvailableProductTypes if the model state is invalid
            viewModel.AvailableProductTypes = _context.ProductTypes.ToList();
            return View(viewModel);
        }


        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
