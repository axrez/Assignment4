using System.ComponentModel.DataAnnotations;

namespace Assignment4.Entities
{
    public class Tag
    {
        public int Id { get; set; }

        [MaxLength(50)]
        [Required]
        [Key]
        public string Name { get; set; }

        public Task[] tasks { get; set; }
    }
}
