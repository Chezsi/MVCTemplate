using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCTemplate.Models;
using MVCTemplate.Util;
using MVCTemplate.ViewModels;
using System.Diagnostics;
using MVCtemplate.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MVCTemplate.ViewModels.MVCTemplate.ViewModels;

namespace MVCTemplate.Areas.Admin.Controllers
{
    [Authorize(Roles = $"{Roles.Admin}")]
    [Area("Admin")]
    public class SiteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SiteController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var vm = new SiteVM
            {
                Sites = _context.Sites.ToList(),
                NewSite = new Site()
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult Create(SiteVM vm)
        {
            var branchName = vm.NewSite?.Branch?.Trim();

            bool exists = !string.IsNullOrEmpty(branchName) &&
                _context.Sites.Any(s => s.Branch.ToLower() == branchName.ToLower());

            if (exists)
            {
                return Json(new { success = false, message = "This branch name already exists." });
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Please fill out all required fields." });
            }

            _context.Sites.Add(vm.NewSite);
            _context.SaveChanges();

            return Json(new { success = true, message = "Site added successfully." });
        }


        [HttpPost]
        public IActionResult Edit(Site site)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var dbSite = _context.Sites.FirstOrDefault(s => s.Id == site.Id);
            if (dbSite == null)
                return Json(new { success = false, message = "Site not found." });

            dbSite.Branch = site.Branch;
            dbSite.Location = site.Location;
            dbSite.GenerateUpdatedAt();

            _context.SaveChanges();

            return Json(new { success = true, message = "Site updated successfully." });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var site = _context.Sites.Find(id);
            if (site == null)
            {
                return Json(new { success = false, message = "Site not found." });
            }

            _context.Sites.Remove(site);
            _context.SaveChanges();

            return Json(new { success = true, message = "Site deleted successfully." });
        }

        [HttpGet]
        public IActionResult GetAllSites()
        {
            var sites = _context.Sites
                .Select(s => new {
                    s.Id,
                    s.Branch,
                    s.Location,
                    CreatedAt = s.CreatedAt.ToString("MMM dd, yyyy"),
                    UpdatedAt = s.UpdatedAt.ToString("MMM dd, yyyy")
                })
                .ToList();

            return Json(new { data = sites });
        }
        
    }
}
