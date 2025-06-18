using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MVCTemplate.Models
{
    public class Report
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Column(TypeName="nvarchar(50)")]
        public string Title { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(100)")]
        public string ImageName { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Report()
        {
            CreatedAt = DateTime.Now;
        }

        public void GenerateUpdatedAt() 
        {
            this.UpdatedAt = DateTime.Now;
        }
    }
}
