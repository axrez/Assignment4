using System.ComponentModel.DataAnnotations;
using Assignment4.Core;

namespace Assignment4.Entities
{
    public class Task
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { set; get; }

        public string Description { get; set; }

        public State state { get; set; }

        public Tag[] tags { get; set; }
    }
}
