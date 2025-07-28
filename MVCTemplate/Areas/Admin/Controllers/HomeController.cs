using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCTemplate.Models;
using MVCTemplate.Util;
using MVCTemplate.ViewModels;
using System.Diagnostics;
using MVCtemplate.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace MVCTemplate.Areas.Admin.Controllers
{
    [Authorize(Roles = $"{Roles.Admin},{Roles.User}")]
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {

            var today = DateTime.Today;

            var ageDistribution = _context.Reports
                .AsNoTracking()
                .ToList() // force client-side evaluation
                .GroupBy(r => (int)(today - r.CreatedAt).TotalDays)
                .OrderByDescending(g => g.Key)
                .ToDictionary(
                    g => g.Key,
                    g => g.Count()
                );

            var priorityDistribution = _context.Packages
                .AsNoTracking()
                .ToList()
                .GroupBy(p => p.Priority)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.Count());

            var dashboard = new DashboardVM
            {
                ProductStats = new ProductDashboardVM
                {
                    ProductCount = await _context.Products.CountAsync()
                },
                PackageStats = new PackageDashboardVM
                {
                    PackageCount = await _context.Packages.CountAsync()
                },
                ReportStats = new ReportDashboardVM
                {
                    ReportCount = await _context.Reports.CountAsync()
                },
                ReportAgeDistribution = ageDistribution,
                PackagePriorityDistribution = priorityDistribution
            };

            return View(dashboard);
        }


        public IActionResult Privacy()
        {
            return View();
        }
    }
}
