using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MVCTemplate.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVCTemplate.ViewModels
{
    public class PersonVM
    {
        public Person Person { get; set; }

        public IEnumerable<SelectListItem> CategoryList { get; set; }

        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Position { get; set; }

        public int CategoryId { get; set; } // for foreign key
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
    }
}
