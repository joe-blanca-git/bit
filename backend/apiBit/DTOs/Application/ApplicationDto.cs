using System.ComponentModel.DataAnnotations;

namespace apiBit.DTOs
{
    public class ApplicationDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
    }
}