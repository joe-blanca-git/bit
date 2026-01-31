using System.ComponentModel.DataAnnotations;

namespace apiBit.DTOs
{
    public class MenuDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public string Route { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        
        public Guid? ParentMenuId { get; set; } 
        
        [Required]
        public Guid ApplicationId { get; set; }
    }
}