using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Assignment4.Core;

namespace Assignment4.Entities
{
    public class Task
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { set; get; }

        public string Description { get; set; }

        public State state { get; set; }

        public ICollection<Tag> tags { get; set; }
    }
}
