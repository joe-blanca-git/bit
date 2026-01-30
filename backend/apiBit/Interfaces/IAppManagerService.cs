using apiBit.DTOs;
using apiBit.Models;

namespace apiBit.Interfaces
{
    public interface IAppManagerService
    {
        // Apps
        Task<List<Application>> GetAllApps();
        Task<Application> CreateApp(ApplicationDto model);

        // Menus (Pai)
        Task<ApplicationMenu> CreateMenu(MenuDto model);
        
        // SubMenus (Filho)
        Task<ApplicationSubMenu> CreateSubMenu(MenuDto model);

        // Delete (Genérico pelo ID)
        Task<bool> DeleteApp(Guid id);
        Task<bool> DeleteMenu(Guid id);
        Task<bool> DeleteSubMenu(Guid id);

        Task<bool> SetPlanPermissions(PlanPermissionDto model);
    }
}