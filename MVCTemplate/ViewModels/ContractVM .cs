using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MVCTemplate.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVCTemplate.ViewModels
{
    public class ContractVM
    {
        public Models.Contract Contract { get; set; } = new Models.Contract();


        public IEnumerable<SelectListItem> PersonList { get; set; } = new List<SelectListItem>();
    }
}
