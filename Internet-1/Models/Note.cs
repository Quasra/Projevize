using System.ComponentModel.DataAnnotations;

namespace Internet_1.Models
{
    public class Note
    {
        [Key] // Birincil anahtar
        public int Id { get; set; }

        [Required] // Zorunlu alan
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }
    }
}
