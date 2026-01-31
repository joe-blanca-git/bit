namespace apiBit.DTOs
{
    public class PlanPermissionDto
    {
        public Guid PlanId { get; set; }
        
        // Listas de IDs que esse plano terá acesso
        public List<Guid> ApplicationIds { get; set; } = new();
        public List<Guid> MenuIds { get; set; } = new();
        public List<Guid> SubMenuIds { get; set; } = new();
    }
}