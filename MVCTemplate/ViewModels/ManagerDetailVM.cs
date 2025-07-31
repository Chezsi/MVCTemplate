using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MVCTemplate.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVCTemplate.ViewModels
{
    public class ManagerDetailVM
    {
        public int ManagerId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Branch { get; set; }
        public string Location { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Product Product { get; set; } = new Product();
    }
}
