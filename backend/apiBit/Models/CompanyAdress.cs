using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace apiBit.Models
{
    public class CompanyAddress
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(10)]
        public string ZipCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Street { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string Number { get; set; } = string.Empty;

        public string? Complement { get; set; }

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [MaxLength(2)]
        public string State { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Neighborhood { get; set; } = string.Empty;

        // Chave Estrangeira para Company
        public Guid CompanyId { get; set; }
        
        [JsonIgnore]
        public Company? Company { get; set; }
    }
}