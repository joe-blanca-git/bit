using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace apiBit.Models
{
    public class Plan
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        // "decimal(18,2)" significa 18 dígitos no total, sendo 2 decimais
        [Required]
        [Column(TypeName = "decimal(18,2)")] 
        public decimal Price { get; set; }

        // Pode ser nulo caso não tenha preço promocional no momento
        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscountPrice { get; set; }

        public List<PlanApplication> AllowedApps { get; set; } = new();
        public List<PlanMenu> AllowedMenus { get; set; } = new();
        public List<PlanSubMenu> AllowedSubMenus { get; set; } = new();
    }
}