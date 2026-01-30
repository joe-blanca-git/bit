namespace apiBit.DTOs
{
    // 1. O Objeto Principal (O Plano)
    public class PlanResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }

        // Lista de Apps permitidos (que terão menus dentro)
        public List<PlanAppTreeDto> AllowedApps { get; set; } = new();
    }

    // 2. O App (que tem Menus)
    public class PlanAppTreeDto
    {
        public Guid Id { get; set; } // ID do vinculo PlanApplication
        public Guid PlanId { get; set; }
        public Guid ApplicationId { get; set; }
        public string ApplicationName { get; set; } = string.Empty; // Útil para o front

        // Lista de Menus permitidos DESTE App
        public List<PlanMenuTreeDto> AllowedMenus { get; set; } = new();
    }

    // 3. O Menu (que tem SubMenus)
    public class PlanMenuTreeDto
    {
        public Guid ApplicationMenuId { get; set; }
        public string Title { get; set; } = string.Empty;

        // Lista de SubMenus permitidos DESTE Menu
        public List<PlanSubMenuTreeDto> AllowedSubMenus { get; set; } = new();
    }

    // 4. O SubMenu (Fim da linha)
    public class PlanSubMenuTreeDto
    {
        public Guid ApplicationSubMenuId { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}