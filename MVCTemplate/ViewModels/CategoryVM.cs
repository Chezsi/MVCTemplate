using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using MVCTemplate.Models;

namespace MVCTemplate.ViewModels
{
    public class CategoryVM
    {
        public Category Category { get; set; } = new Category
        {
            NameCategory = string.Empty,
            CodeCategory = string.Empty
        };

        // Optional: to show associated persons in the view
        public IEnumerable<Person> Persons { get; set; } = new List<Person>();

        // Optional: if you ever need a dropdown of categories
        public IEnumerable<SelectListItem> CategoryList { get; set; } = new List<SelectListItem>();
    }
}
