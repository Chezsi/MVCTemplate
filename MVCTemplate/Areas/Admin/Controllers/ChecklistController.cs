using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCtemplate.DataAccess.Data;
using MVCTemplate.Models;
using MVCTemplate.Util;

namespace MVCTemplate.Controllers
{
    [Authorize(Roles = $"{Roles.Admin}, {Roles.User}")]
    [Area("Admin")]
    public class ChecklistController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChecklistController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: List all checklists
        [HttpGet("Admin/Checklist/Checklist")]
        public async Task<IActionResult> Checklist()
        {
            var checklists = await _context.Checklists.ToListAsync();
            return View(checklists);
        }

        // POST: Create a new checklist item
        [HttpPost]
        public async Task<IActionResult> Create(string name, string category, string forRole)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Json(new { success = false, message = "Name is required." });
            }

            if (string.IsNullOrWhiteSpace(category))
            {
                return Json(new { success = false, message = "Category is required." });
            }

            if (string.IsNullOrWhiteSpace(forRole))
            {
                return Json(new { success = false, message = "ForRole is required." });
            }

            name = name.Trim().ToUpper();

            // Check uniqueness (case-insensitive)
            bool nameExists = await _context.Checklists
                .AnyAsync(c => c.Name.ToLower() == name.ToLower());

            if (nameExists)
            {
                return Json(new { success = false, message = "A checklist item with this name already exists." });
            }

            var checklist = new Checklist
            {
                Name = name,
                Category = category,
                ForRole = forRole,
                Status = true
            };

            _context.Checklists.Add(checklist);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Checklist item added successfully!" });
        }

        // POST: Disable a checklist item
        [HttpPost]
        public async Task<IActionResult> Disable(int id)
        {
            var checklist = await _context.Checklists.FindAsync(id);
            if (checklist == null)
                return NotFound();

            checklist.Status = false; // mark as disabled
            await _context.SaveChangesAsync();

            // Redirect back to Checklist view
            return RedirectToAction(nameof(Checklist));
        }

        // POST: Restore a checklist item
        [HttpPost]
        public async Task<IActionResult> Restore(int id)
        {
            var checklist = await _context.Checklists.FindAsync(id);
            if (checklist == null)
                return NotFound();

            checklist.Status = true; // mark as active
            await _context.SaveChangesAsync();

            // Redirect back to Checklist view
            return RedirectToAction(nameof(Checklist));
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string name, string category, string forRole)
        {
            var checklist = await _context.Checklists.FindAsync(id);
            if (checklist == null)
            {
                return Json(new { success = false, message = "Checklist item not found." });
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                return Json(new { success = false, message = "Name is required." });
            }

            if (string.IsNullOrWhiteSpace(category))
            {
                return Json(new { success = false, message = "Category is required." });
            }

            if (string.IsNullOrWhiteSpace(forRole))
            {
                return Json(new { success = false, message = "ForRole is required." });
            }

            name = name.Trim().ToUpper();

            // Check uniqueness (exclude the current record)
            bool nameExists = await _context.Checklists
                .AnyAsync(c => c.Id != id && c.Name.ToLower() == name.ToLower());

            if (nameExists)
            {
                return Json(new { success = false, message = "A checklist item with this name already exists." });
            }

            // Update fields
            checklist.Name = name;
            checklist.Category = category;
            checklist.ForRole = forRole;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Checklist item updated successfully!" });
        }
    }
}