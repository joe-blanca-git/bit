using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using apiBit.API.Models;

namespace apiBit.Models
{
    public class Person
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "O nome completo é obrigatório")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(14)]
        public string Document { get; set; } = string.Empty;

        public DateTime BirthDate { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;
        
        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}