using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCTemplate.Models
{
    public class Contract
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Contract Name")]
        public string Name { get; set; }

        [Required]
        [DisplayName("Contract Description")]
        public string Description { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [DisplayName("Contract Validity")]
        public DateTime? Validity { get; set; }

        [Required]
        public int PersonId { get; set; }

        [ForeignKey("PersonId")]
        public Person? Person { get; set; } // optional: if you have a navigation property

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Contract()
        {
            CreatedAt = DateTime.Now;
        }

        public void GenerateUpdatedAt()
        {
            this.UpdatedAt = DateTime.Now;
        }
    }
}
