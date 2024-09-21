using _21BITV03_Nhom04_website_clothes.Data;
using _21BITV03_Nhom04_website_clothes.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace _21BITV03_Nhom04_website_clothes.Controllers
{
    [Authorize]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly WebsiteClothesContext _context;

        public UserController(WebsiteClothesContext context)
        {
            _context = context;

        }
        public async Task<IActionResult> Index()
        {
            var users = await _context.AspNetUsers
                .Select(user => new User
                {
                    Users = user,
                    UserInfo = user.UserInfo,
                    Roles = _context.AspNetRoles
                        .Where(role => role.Users.Contains(user))
                        .ToList()
                })
                .ToListAsync();

            return View(users);
        }

        public IActionResult Create()
        {
            // Load roles to a dropdown list in the view
            ViewBag.Roles = _context.AspNetRoles.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if the username already exists
                var existingUser = await _context.AspNetUsers.FirstOrDefaultAsync(u => u.UserName == model.UserName);
                if (existingUser != null)
                {
                    // Add a validation error to ModelState
                    ModelState.AddModelError("UserName", "Username is already taken. Please choose a different one.");

                    // Reload roles and return the view with the error
                    ViewBag.Roles = _context.AspNetRoles.ToList();
                    return View(model);
                }

                var user = new AspNetUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    PasswordHash = model.Password, // Consider hashing the password before storing it
                    PhoneNumber = model.Phone.ToString(), // Assuming Phone is stored as a string
                    UserInfo = new UserInfo
                    {
                        FullName = model.FullName,
                        Address = model.Address,
                        Phone = model.Phone,
                        Email = model.Email,      // Adding Email to UserInfo
                        Password = model.Password, // Adding Password to UserInfo
                        UserName = model.UserName  // Adding UserName to UserInfo
                    }
                };

                // Assign role to user
                var role = await _context.AspNetRoles.FindAsync(model.RoleId);
                if (role != null)
                {
                    user.Roles.Add(role);
                }

                _context.AspNetUsers.Add(user);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            // Reload roles if validation fails
            ViewBag.Roles = _context.AspNetRoles.ToList();
            return View(model);
        }


        // GET: Edit User
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.AspNetUsers
                .Include(u => u.UserInfo)
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var model = new UserViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Phone = int.Parse(user.PhoneNumber), // Assuming phone number is stored as an integer
                FullName = user.UserInfo.FullName,
                Address = user.UserInfo.Address,
            };

            ViewBag.Roles = _context.AspNetRoles.ToList();
            return View(model);
        }

        // POST: Edit User
        [HttpPost]
        public async Task<IActionResult> Edit(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if the username is unique (excluding the current user)
                var existingUser = await _context.AspNetUsers
                    .FirstOrDefaultAsync(u => u.UserName == model.UserName && u.Id != model.UserId);

                if (existingUser != null)
                {
                    ModelState.AddModelError("UserName", "Username is already taken. Please choose a different one.");
                    ViewBag.Roles = _context.AspNetRoles.ToList();
                    return View(model);
                }

                // Fetch user with UserInfo and existing roles
                var user = await _context.AspNetUsers
                    .Include(u => u.UserInfo)
                    .Include(u => u.Roles) // Include user roles to manage role changes
                    .FirstOrDefaultAsync(u => u.Id == model.UserId);

                if (user == null)
                {
                    return NotFound();
                }

                // Update user details
                user.UserName = model.UserName;
                user.Email = model.Email;
                user.PasswordHash = model.Password; // Consider hashing the password before storing it
                user.PhoneNumber = model.Phone.ToString();

                // Update UserInfo details
                user.UserInfo.FullName = model.FullName;
                user.UserInfo.Address = model.Address;
                user.UserInfo.Phone = model.Phone;
                user.UserInfo.Email = model.Email;
                user.UserInfo.UserName = model.UserName;
                user.UserInfo.Password = model.Password; // Consider hashing



                _context.AspNetUsers.Update(user);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            // Reload roles if validation fails
            ViewBag.Roles = _context.AspNetRoles.ToList();
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.AspNetUsers
                .Include(u => u.UserInfo)
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var model = new UserViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Phone = int.Parse(user.PhoneNumber), // Assuming phone number is stored as an integer
                FullName = user.UserInfo.FullName,
                Address = user.UserInfo.Address,
            };

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.AspNetUsers
                .Include(u => u.UserInfo)
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            _context.AspNetUsers.Remove(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index)); // Redirect to an appropriate action after deletion
        }



    }
}
