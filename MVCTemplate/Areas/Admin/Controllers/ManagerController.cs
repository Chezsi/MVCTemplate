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
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var managers = _context.Managers.ToList();
            return View(managers);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Manager manager)
        {
            if (ModelState.IsValid)
            {
                _context.Managers.Add(manager);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(manager);
        }

        public IActionResult Edit(int id)
        {
            var manager = _context.Managers.Find(id);
            if (manager == null) return NotFound();
            return View(manager);
        }

        [HttpPost]
        public IActionResult Edit(Manager manager)
        {
            if (ModelState.IsValid)
            {
                manager.GenerateUpdatedAt();
                _context.Managers.Update(manager);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(manager);
        }

        public IActionResult Delete(int id)
        {
            var manager = _context.Managers.Find(id);
            if (manager == null) return NotFound();
            return View(manager);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var manager = _context.Managers.Find(id);
            if (manager == null) return NotFound();

            _context.Managers.Remove(manager);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
