using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MVCTemplate.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVCTemplate.ViewModels
{
    public class DashboardVM
    {
        public ProductDashboardVM ProductStats { get; set; }
        public PackageDashboardVM PackageStats { get; set; }
        public ReportDashboardVM ReportStats { get; set; }
    }
}
