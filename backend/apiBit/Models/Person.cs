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
        [MaxLength(20)]
        public string Document { get; set; } = string.Empty;

        public DateTime BirthDate { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneSecondary { get; set; }

        [MaxLength(100)]
        [EmailAddress]
        public string? EmailSecondary { get; set; }

        [MaxLength(100)]
        public string? Position { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public User? User { get; set; }
        public ICollection<PersonAddress> Addresses { get; set; } = new List<PersonAddress>();
    }
}