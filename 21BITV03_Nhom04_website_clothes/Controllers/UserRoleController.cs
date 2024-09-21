using Microsoft.AspNetCore.Mvc;
using _21BITV03_Nhom04_website_clothes.Data;
using _21BITV03_Nhom04_website_clothes.Models;
using System.Linq;
using _21BITV03_Nhom04_website_clothes.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace _21BITV03_Nhom04_website_clothes.Controllers
{
    [Authorize]
    [Authorize(Roles = "Admin")]
    public class UserRoleController : Controller
    {
        private readonly WebsiteClothesContext _context;

        public UserRoleController(WebsiteClothesContext context)
        {
            _context = context;
        }

        // GET: UserRole/Index
        public IActionResult Index()
        {
            var viewModel = _context.AspNetUsers
                .Select(user => new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Roles = user.Roles.Select(role => new RoleViewModel
                    {
                        RoleId = role.Id,
                        RoleName = role.Name,
                        IsSelected = user.Roles.Contains(role)
                    }).ToList()
                }).ToList();

            return View(viewModel);
        }
        // GET: UserRole/Edit/5
        public IActionResult Edit(int id)
        {
            var user = _context.AspNetUsers
                .Where(u => u.Id == id)
                .Select(u => new UserRoleViewModel
                {
                    UserId = u.Id,
                    UserName = u.UserName,
                    Roles = _context.AspNetRoles.Select(role => new RoleViewModel
                    {
                        RoleId = role.Id,
                        RoleName = role.Name,
                        IsSelected = u.Roles.Any(r => r.Id == role.Id)
                    }).ToList()
                }).FirstOrDefault();

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(UserRoleViewModel model)
        {
            // Fetch the user from the database
            var user = _context.AspNetUsers.Include(u => u.Roles).FirstOrDefault(u => u.Id == model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            // Clear existing roles

            // Get selected role ID from the form submission
            var selectedRoleId = model.SelectedRoleId;

            if (selectedRoleId != 0)  // Check if a valid role ID is selected
            {           

                // Fetch the role from the database
                var role = _context.AspNetRoles.Find(selectedRoleId);
                if (role != null)
                {
                    user.Roles.Clear();

                    // Add the role to the user's roles
                    user.Roles.Add(role);
                }
            }

            // Save changes to the database
            _context.SaveChanges();

            // Redirect to the Index action
            return RedirectToAction(nameof(Index));
        }




    }
}
