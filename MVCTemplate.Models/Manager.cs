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
    public class Manager
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Branch { get; set; }
        public int Location { get; set; }

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
