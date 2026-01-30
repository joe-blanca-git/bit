using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace apiBit.Models
{
    public class PlanApplication
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid PlanId { get; set; }
        [JsonIgnore]
        [ForeignKey("PlanId")]
        public Plan? Plan { get; set; }

        public Guid ApplicationId { get; set; }
        [JsonIgnore] // Evita ciclo infinito no JSON
        [ForeignKey("ApplicationId")]
        public Application? Application { get; set; }
    }
}