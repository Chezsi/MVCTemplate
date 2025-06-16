using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCTemplate.Models
{
    public class Person
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Position { get; set; }

        public int? CategoryId { get; set; } // for foreign key
        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Person()
        {
            CreatedAt = DateTime.Now;
        }

        public void GenerateUpdatedAt()
        {
            this.UpdatedAt = DateTime.Now;
        }
    }
}
