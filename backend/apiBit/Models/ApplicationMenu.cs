using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace apiBit.Models
{
    public class ApplicationMenu
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(250)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Route { get; set; } = string.Empty; 

        [MaxLength(50)]
        public string Icon { get; set; } = string.Empty;

        public Guid ApplicationId { get; set; }

        [JsonIgnore]
        [ForeignKey("ApplicationId")]
        public Application? Application { get; set; }

        public List<ApplicationSubMenu> SubMenus { get; set; } = new();
    }
}