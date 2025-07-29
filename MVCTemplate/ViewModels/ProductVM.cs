using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MVCTemplate.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVCTemplate.ViewModels
{
    public class ProductVM
    {
        public Product Product { get; set; } = new Product { Name = "" }; // to keep required in the model

        public IEnumerable<SelectListItem> Managers { get; set; } = new List<SelectListItem>();
    }
}
