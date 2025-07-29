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

        // Main page
        public IActionResult Index()
        {
            var sites = _context.Sites.ToList();
            return View(sites);
        }

        // GET: Create View (loaded in modal via AJAX)
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create
        [HttpPost]
        public IActionResult Create(Site site)
        {
            if (ModelState.IsValid)
            {
                _context.Sites.Add(site);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(site);
        }

        // GET: Edit View (loaded in modal via AJAX)
        public IActionResult Edit(int id)
        {
            var site = _context.Sites.Find(id);
            if (site == null) return NotFound();

            return View(site);
        }

        // POST: Edit
        [HttpPost]
        public IActionResult Edit(Site site)
        {
            if (ModelState.IsValid)
            {
                site.GenerateUpdatedAt();
                _context.Sites.Update(site);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(site);
        }

        // GET: Delete Confirmation View
        public IActionResult Delete(int id)
        {
            var site = _context.Sites.Find(id);
            if (site == null) return NotFound();

            return View(site);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var site = _context.Sites.Find(id);
            if (site == null) return NotFound();

            _context.Sites.Remove(site);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
