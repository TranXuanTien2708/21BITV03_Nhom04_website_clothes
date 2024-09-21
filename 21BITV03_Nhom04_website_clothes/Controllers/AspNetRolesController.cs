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
    public class AspNetRolesController : Controller
    {
        private readonly WebsiteClothesContext _context;

        public AspNetRolesController(WebsiteClothesContext context)
        {
            _context = context;
        }

        // GET: AspNetRoles
        public async Task<IActionResult> Index()
        {
            return View(await _context.AspNetRoles.ToListAsync());
        }

        // GET: AspNetRoles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aspNetRole = await _context.AspNetRoles
                .FirstOrDefaultAsync(m => m.Id == id);
            if (aspNetRole == null)
            {
                return NotFound();
            }

            return View(aspNetRole);
        }

        // GET: AspNetRoles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AspNetRoles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] AspNetRole aspNetRole)
        {
            if (ModelState.IsValid)
            {
                // Loại bỏ khoảng trắng và chuẩn hóa tên
                aspNetRole.Name = aspNetRole.Name.Trim(); // Xóa khoảng trắng đầu và cuối
                aspNetRole.NormalizedName = aspNetRole.Name.Replace(" ", "").ToUpper(); // Loại bỏ khoảng trắng và chuyển thành chữ hoa
                aspNetRole.ConcurrencyStamp = Guid.NewGuid().ToString(); // Tạo ConcurrencyStamp
                _context.Add(aspNetRole);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(aspNetRole);
        }


        // GET: AspNetRoles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aspNetRole = await _context.AspNetRoles.FindAsync(id);
            if (aspNetRole == null)
            {
                return NotFound();
            }
            return View(aspNetRole);
        }

        // POST: AspNetRoles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] AspNetRole aspNetRole)
        {
            if (id != aspNetRole.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var role = await _context.AspNetRoles.FindAsync(id);
                    if (role != null)
                    {
                        // Loại bỏ khoảng trắng và chuẩn hóa tên
                        role.Name = aspNetRole.Name.Trim(); // Xóa khoảng trắng đầu và cuối
                        role.NormalizedName = role.Name.Replace(" ", "").ToUpper(); // Loại bỏ khoảng trắng và chuyển thành chữ hoa
                        _context.Update(role);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AspNetRoleExists(aspNetRole.Id))
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
            return View(aspNetRole);
        }


        // GET: AspNetRoles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aspNetRole = await _context.AspNetRoles
                .FirstOrDefaultAsync(m => m.Id == id);
            if (aspNetRole == null)
            {
                return NotFound();
            }

            return View(aspNetRole);
        }

        // POST: AspNetRoles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var aspNetRole = await _context.AspNetRoles.FindAsync(id);
            if (aspNetRole != null)
            {
                _context.AspNetRoles.Remove(aspNetRole);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AspNetRoleExists(int id)
        {
            return _context.AspNetRoles.Any(e => e.Id == id);
        }
    }

}
