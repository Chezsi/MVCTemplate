using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MVCTemplate.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVCTemplate.ViewModels
{
    namespace MVCTemplate.ViewModels
    {
        public class EditUserVM
        {
            public string OriginalEmail { get; set; }
            public string Email { get; set; }
            public string Role { get; set; }
            public string NewPassword { get; set; }
        }
    }
}
