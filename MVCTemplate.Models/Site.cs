using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCTemplate.Models
{
    public class Site
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Branch { get; set; }
        [Required]
        public string Location { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Site()
        {
            CreatedAt = DateTime.Now;
        }

        public void GenerateUpdatedAt()
        {
            this.UpdatedAt = DateTime.Now;
        }
    }
}
