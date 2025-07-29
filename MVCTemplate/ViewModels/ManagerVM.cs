using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MVCTemplate.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVCTemplate.ViewModels
{
    public class ManagerVM
    {
        public IEnumerable<Manager> Managers { get; set; } = new List<Manager>();

        // For modal creation (optional use)
        public Manager NewManager { get; set; } = new Manager();

        // Optional: Sites for dropdown list (used during create/edit)
        public IEnumerable<Site> Sites { get; set; } = new List<Site>();
    }
}
