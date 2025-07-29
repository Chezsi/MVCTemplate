using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCTemplate.Models;
using MVCTemplate.Util;
using MVCTemplate.ViewModels;
using MVCtemplate.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using MVCTemplate.ViewModels.MVCTemplate.ViewModels;

namespace MVCTemplate.Areas.Admin.Controllers
{
    [Authorize(Roles = $"{Roles.Admin}")]
    [Area("Admin")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var vm = new ManagerVM
            {
                Sites = _context.Sites.ToList(),
                Managers = _context.Managers.Include(m => m.Site).ToList(),
                NewManager = new MVCTemplate.Models.Manager()
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult Create(ManagerVM vm)
        {
            if (string.IsNullOrWhiteSpace(vm.NewManager?.Name) || string.IsNullOrWhiteSpace(vm.NewManager?.Email))
            {
                return Json(new { success = false, message = "All fields are required." });
            }

            bool nameExists = _context.Managers.Any(m => m.Name.ToLower() == vm.NewManager.Name.Trim().ToLower());
            if (nameExists)
            {
                return Json(new { success = false, message = "This manager name already exists." });
            }

            _context.Managers.Add(vm.NewManager);
            _context.SaveChanges();

            return Json(new { success = true, message = "Manager added successfully." });
        }

        [HttpPost]
        public IActionResult Edit(MVCTemplate.Models.Manager manager)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var dbManager = _context.Managers.FirstOrDefault(m => m.Id == manager.Id);
            if (dbManager == null)
                return Json(new { success = false, message = "Manager not found." });

            bool nameExists = _context.Managers
                .Any(m => m.Id != manager.Id && m.Name.ToLower() == manager.Name.Trim().ToLower());

            if (nameExists)
            {
                return Json(new { success = false, message = "Another manager already uses this name." });
            }

            dbManager.Name = manager.Name;
            dbManager.Email = manager.Email;
            dbManager.SiteId = manager.SiteId;
            dbManager.GenerateUpdatedAt();

            _context.SaveChanges();

            return Json(new { success = true, message = "Manager updated successfully." });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var manager = _context.Managers.Find(id);
            if (manager == null)
            {
                return Json(new { success = false, message = "Manager not found." });
            }

            _context.Managers.Remove(manager);
            _context.SaveChanges();

            return Json(new { success = true, message = "Manager deleted successfully." });
        }

        [HttpGet]
        public IActionResult GetAllManagers()
        {
            var data = _context.Managers
                .Include(m => m.Site)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    email = m.Email,
                    branch = m.Site.Branch,
                    location = m.Site.Location,
                    createdAt = m.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                    updatedAt = m.UpdatedAt == DateTime.MinValue ? "" : m.UpdatedAt.ToString("yyyy-MM-dd HH:mm"),
                    siteId = m.SiteId
                }).ToList();

            return Json(new { data });
        }

    }
}
