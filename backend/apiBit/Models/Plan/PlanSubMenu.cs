using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace apiBit.Models
{
    public class PlanSubMenu
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid PlanId { get; set; }
        [JsonIgnore]
        [ForeignKey("PlanId")]
        public Plan? Plan { get; set; }

        public Guid ApplicationSubMenuId { get; set; }
        [JsonIgnore]
        [ForeignKey("ApplicationSubMenuId")]
        public ApplicationSubMenu? ApplicationSubMenu { get; set; }
    }
}