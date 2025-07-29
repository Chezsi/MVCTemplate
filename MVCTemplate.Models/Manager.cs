using MVCTemplate.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MVCTemplate.Models
{
    public class Manager
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        // Foreign Key
        [Display(Name = "Site")]
        public int SiteId { get; set; }

        [BindNever]
        [ValidateNever]
        [ForeignKey("SiteId")]
        public Site Site { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Manager()
        {
            CreatedAt = DateTime.Now;
        }

        public void GenerateUpdatedAt()
        {
            this.UpdatedAt = DateTime.Now;
        }
    }
}