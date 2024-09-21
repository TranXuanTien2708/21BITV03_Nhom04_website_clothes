using _21BITV03_Nhom04_website_clothes.Data;
using _21BITV03_Nhom04_website_clothes.Helper;
using _21BITV03_Nhom04_website_clothes.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace _21BITV03_Nhom04_website_clothes.Controllers
{
    [Authorize]
    [Authorize(Roles = "Admin")]
    public class SearchController : Controller
    {
        private readonly WebsiteClothesContext _context;

        public SearchController(WebsiteClothesContext context)
        {
            _context = context;
        }
        public IActionResult Index(SearchViewModel model)
        {
            var redirectResult = NavigationHelper.RedirectToRoleBasedPage(this);
            if (redirectResult != null)
            {
                return redirectResult;
            }
            var productTypes = _context.ProductTypes.ToList();
            var colors = _context.ProductColors.ToList();
            var sizes = _context.ProductSizes.ToList();

            // Get the product counts for each product type
            var productCounts = _context.ProductTypeLinks
                .GroupBy(ptl => ptl.ProductTypeId ?? 0)  // Handle nullable int
                .Select(g => new
                {
                    ProductTypeId = g.Key,
                    Count = g.Count()
                })
                .ToDictionary(x => x.ProductTypeId, x => x.Count);

            // Populate the view model
            model.Products = _context.Products.ToList(); // Populate with all products initially
            model.Colors = colors;
            model.Sizes = sizes;
            model.ProductTypes = productTypes;
            model.ProductTypeCounts = productCounts;

            return View(model);
        }




        [HttpPost]
        public IActionResult Search(SearchViewModel model)
        {
            var redirectResult = NavigationHelper.RedirectToRoleBasedPage(this);
            if (redirectResult != null)
            {
                return redirectResult;
            }
            // Ensure model.PageNumber and model.PageSize are set
            model.PageNumber = model.PageNumber > 0 ? model.PageNumber : 1;
            model.PageSize = model.PageSize > 0 ? model.PageSize : 10; // Default page size
            var colors = _context.ProductColors.ToList();
            var sizes = _context.ProductSizes.ToList();
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(model.ProductName))
            {
                query = query.Where(p => p.ProductName.Contains(model.ProductName));
            }

            // Apply other filters as needed

            var totalResults = query.Count();
            var products = query
                .Skip((model.PageNumber - 1) * model.PageSize)
                .Take(model.PageSize)
                .Select(p => new
                {
                    Product = p,
                    SubProduct = p.SubProducts.FirstOrDefault()
                })
                .ToList();

            model.TotalResults = totalResults;
            model.SearchResults = products.Select(x => new Product
            {
                ProductId = x.Product.ProductId,
                ProductName = x.Product.ProductName,
                Description = x.Product.Description,
                SubProducts = new List<SubProduct> { x.SubProduct }
            }).ToList();
            model.Colors = colors;
            model.Sizes = sizes;
            return View("Index", model);
        }
        [HttpGet]
        public IActionResult FilterColor_Size(List<int> colorIds, List<int> sizeIds, double? minPrice, double? maxPrice)
        {
            var redirectResult = NavigationHelper.RedirectToRoleBasedPage(this);
            if (redirectResult != null)
            {
                return redirectResult;
            }
            var filterViewModel = new FilterViewModel
            {
                AvailableColors = _context.ProductColors.ToList(),
                AvailableSizes = _context.ProductSizes.ToList(),
                SelectedColorIds = colorIds ?? new List<int>(),
                SelectedSizeIds = sizeIds ?? new List<int>(),
                MinPrice = minPrice.HasValue ? (decimal?)minPrice.Value : null,
                MaxPrice = maxPrice.HasValue ? (decimal?)maxPrice.Value : null
            };

            var filteredSubProducts = _context.SubProducts.AsQueryable();

            if (colorIds != null && colorIds.Any())
            {
                filteredSubProducts = filteredSubProducts.Where(sp => colorIds.Contains(sp.ColorId.GetValueOrDefault()));
            }

            if (sizeIds != null && sizeIds.Any())
            {
                filteredSubProducts = filteredSubProducts.Where(sp => sizeIds.Contains(sp.SizeId.GetValueOrDefault()));
            }

            // Check if minPrice and maxPrice are set to their default values
            bool isMinPriceDefault = !minPrice.HasValue || minPrice.Value == 0;
            bool isMaxPriceDefault = !maxPrice.HasValue || maxPrice.Value >= 500000; // Update default to 50,000

            if (!isMinPriceDefault || !isMaxPriceDefault)
            {
                // Apply price filtering only if minPrice or maxPrice are not default
                if (!isMinPriceDefault)
                {
                    filteredSubProducts = filteredSubProducts.Where(sp => sp.DiscountedPrice.HasValue && sp.DiscountedPrice.Value >= minPrice.Value);
                }

                if (!isMaxPriceDefault)
                {
                    filteredSubProducts = filteredSubProducts.Where(sp => sp.DiscountedPrice.HasValue && sp.DiscountedPrice.Value <= maxPrice.Value);
                }
            }

            var filteredProducts = filteredSubProducts
                .Select(sp => sp.MainProduct)
                .Distinct()
                .Include(mp => mp.SubProducts)
                .ToList();

            filterViewModel.FilteredProducts = filteredProducts;

            return PartialView("FilterColor_Size", filterViewModel); // Return a partial view to load into the page
        }






    }
}
