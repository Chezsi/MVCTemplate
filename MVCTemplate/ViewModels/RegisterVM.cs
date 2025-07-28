using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MVCTemplate.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVCTemplate.ViewModels
{
    public class RegisterVM
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
