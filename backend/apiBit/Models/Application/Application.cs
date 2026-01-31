using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace apiBit.Models
{
    public class Application
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public List<ApplicationMenu> Menus { get; set;} = new();
    }
}