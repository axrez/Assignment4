using System.ComponentModel.DataAnnotations;

namespace Assignment4.Entities
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [Key]
        [MaxLength(100)]
        public string Email { get; set; }

        public Task[] tasks { get; set; }
    }
}
