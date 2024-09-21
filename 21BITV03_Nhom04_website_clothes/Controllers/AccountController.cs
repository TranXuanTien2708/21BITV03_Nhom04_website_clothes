using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Security.Claims;
using _21BITV03_Nhom04_website_clothes.Models;
using Microsoft.AspNetCore.Authorization;
using _21BITV03_Nhom04_website_clothes.Data;
using Microsoft.EntityFrameworkCore;
using _21BITV03_Nhom04_website_clothes.Helper;

namespace _21BITV03_Nhom04_website_clothes.Controllers
{
    public class AccountController : Controller
    {
        private readonly WebsiteClothesContext _context;


        public AccountController(WebsiteClothesContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var redirectResult = NavigationHelper.RedirectToRoleBasedPage(this);
            if (redirectResult != null)
            {
                return redirectResult;
            }
            var username = HttpContext.User.Identity.Name; // Get the username from the context
            if (username == null)
            {
                return Json(new { success = false, message = "User is not logged in." });
            }

            // Retrieve the user information based on the username
            var userInfo = await _context.UserInfos
                .Include(u => u.User) // Include AspNetUser details
                .Include(u => u.Orders) // Include orders associated with the user
                    .ThenInclude(o => o.OrderProductLists) // Include order product details
                .Where(u => u.UserName == username)
                .FirstOrDefaultAsync();

            if (userInfo == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            // Prepare the AccountViewModel
            var accountViewModel = new AccountViewModel
            {
                Users = userInfo.User, // AspNetUser information
                UserInfoes = userInfo, // UserInfo information
                Orders = userInfo.Orders.FirstOrDefault(), // Get the first order as an example
                OrderProductLists = userInfo.Orders.SelectMany(o => o.OrderProductLists).ToList() // All order product details
            };

            return View(accountViewModel); // Pass the ViewModel to the view
        }

        public async Task<IActionResult> LogOut()
        {

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
        [HttpGet]
        public IActionResult IsUserLoggedIn()
        {
            return Json(User.Identity.IsAuthenticated);
        }
        public IActionResult Login()
        {
            ClaimsPrincipal claimUser = HttpContext.User;

            // If the user is authenticated, redirect to the Index action
            if (claimUser.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Account");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel modelLogin)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ValidateMessage"] = "Please enter valid data.";
                return View(modelLogin);
            }

            var user = await _context.AspNetUsers
                .Include(u => u.Roles) // Include the Roles navigation property
                .FirstOrDefaultAsync(u => u.UserName == modelLogin.UserName);

            if (user != null && modelLogin.UserName == user.UserName && modelLogin.PassWord == user.PasswordHash)
            {
                // Check the user's roles
                var userRole = user.Roles.FirstOrDefault();
                var roleName = userRole?.Name;

                List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, modelLogin.UserName),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, roleName ?? "User") // Add role claim
        };

                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                AuthenticationProperties properties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    IsPersistent = modelLogin.KeepLoggedIn
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);

                // Redirect based on role
                if (roleName == "Admin")
                {
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    return RedirectToAction("Index", "Home"); // or other default page for non-admin users
                }
            }

            ViewData["ValidateMessage"] = "User not found or password incorrect.";
            return View(modelLogin);
        }

        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.Roles = _context.AspNetRoles.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(UserViewModel model)
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
                var role = await _context.AspNetRoles.FirstOrDefaultAsync(r => r.Name == "user");
                if (role != null)
                {
                    user.Roles.Add(role);
                }

                _context.AspNetUsers.Add(user);
                await _context.SaveChangesAsync();

                return RedirectToAction("Login");
            }

            ViewBag.Roles = _context.AspNetRoles.ToList();
            return View(model);
        }
        public IActionResult AccessDenied()
        {
            return View();
        }

    }
}
