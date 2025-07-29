using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCTemplate.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        
        [DisplayName("Product Name")]
        public string Name { get; set; }
        [DisplayName("Product Description")]
        public string? Description { get; set; }
        [DisplayName("Product Quantity")]
        public int Quantity { get; set; }

        [Required]
        [DisplayName("Assigned Manager")]
        public int? ManagerId { get; set; } //Made it nullable in migration

        [ForeignKey("ManagerId")]
        [ValidateNever]
        [BindNever]
        public Manager Manager { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Product()
        {
            CreatedAt = DateTime.Now;
        }

        public void GenerateUpdatedAt() 
        {
            this.UpdatedAt = DateTime.Now;
        }
    }
}
