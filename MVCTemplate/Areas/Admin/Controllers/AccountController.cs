using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCTemplate.Models;
using MVCTemplate.Util;
using MVCTemplate.ViewModels;
using System.Diagnostics;
using MVCtemplate.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace MVCTemplate.Areas.Admin.Controllers
{
    [Authorize(Roles = $"{Roles.Admin}")]
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.Email
            };

            var result = await _userManager.CreateAsync(user, Roles.Default_Password);

            if (result.Succeeded)
            {
                // Ensure "user" role exists
                if (!await _roleManager.RoleExistsAsync(Roles.User))
                {
                    await _roleManager.CreateAsync(new IdentityRole(Roles.User));
                }

                // Assign role
                await _userManager.AddToRoleAsync(user, Roles.User);

                return RedirectToAction("RegisterSuccess");
            }

            // Handle errors
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // GET: /Account/RegisterSuccess
        public IActionResult RegisterSuccess()
        {
            return View();
        }
    }
}
