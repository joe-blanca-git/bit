using System.ComponentModel.DataAnnotations;

namespace apiBit.DTOs
{
    public class PersonDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Document { get; set; } = string.Empty;

        public DateTime BirthDate { get; set; }

        public string Phone { get; set; } = string.Empty;
        
        public string? PhoneSecondary { get; set; }
        
        [EmailAddress]
        public string? EmailSecondary { get; set; }
        
        public string? Position { get; set; }

        public List<AddressDto> Addresses { get; set; } = new List<AddressDto>();
    }
}