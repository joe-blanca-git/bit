using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace apiBit.Models
{
    public class PersonAddress
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(10)]
        public string ZipCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Street { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string Number { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Complement { get; set; }

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [MaxLength(100)]
        public string State { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Neighborhood { get; set; } = string.Empty;

        [Required]
        public Guid PersonId { get; set; }

        [ForeignKey("PersonId")]
        [JsonIgnore]
        public Person? Person { get; set; }
    }
}