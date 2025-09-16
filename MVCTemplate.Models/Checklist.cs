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
    public class Checklist
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public bool Status { get; set; }
        [Required]
        public string Category { get; set; }
        [Required]
        public string ForRole { get; set; }
    }
}
