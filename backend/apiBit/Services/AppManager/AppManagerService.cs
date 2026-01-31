using apiBit.Data;
using apiBit.DTOs;
using apiBit.Interfaces;
using apiBit.Models;
using Microsoft.EntityFrameworkCore;

namespace apiBit.Services
{
    public class AppManagerService : IAppManagerService
    {
        private readonly AppDbContext _context;

        public AppManagerService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Application>> GetAllApps()
        {
            // Traz tudo aninhado: App -> Menus -> SubMenus
            return await _context.Applications
                .Include(a => a.Menus)
                .ThenInclude(m => m.SubMenus)
                .ToListAsync();
        }

        public async Task<Application> CreateApp(ApplicationDto model)
        {
            var app = new Application { Name = model.Name };
            _context.Applications.Add(app);
            await _context.SaveChangesAsync();
            return app;
        }

        public async Task<ApplicationMenu> CreateMenu(MenuDto model)
        {
            // Valida se o App existe
            var appExists = await _context.Applications.AnyAsync(a => a.Id == model.ApplicationId);
            if (!appExists) throw new Exception("Application not found");

            var menu = new ApplicationMenu
            {
                Title = model.Title,
                Description = model.Description,
                Route = model.Route,
                Icon = model.Icon,
                ApplicationId = model.ApplicationId
            };

            _context.ApplicationMenus.Add(menu);
            await _context.SaveChangesAsync();
            return menu;
        }

        public async Task<ApplicationSubMenu> CreateSubMenu(MenuDto model)
        {
            if (model.ParentMenuId == null) throw new Exception("ParentMenuId is required for SubMenus");

            // Valida se o Menu Pai existe
            var parentMenu = await _context.ApplicationMenus.FindAsync(model.ParentMenuId);
            if (parentMenu == null) throw new Exception("Parent Menu not found");

            var subMenu = new ApplicationSubMenu
            {
                Title = model.Title,
                Description = model.Description,
                Route = model.Route,
                Icon = model.Icon,
                ApplicationMenuId = model.ParentMenuId.Value,
                ApplicationId = model.ApplicationId 
            };

            _context.ApplicationSubMenus.Add(subMenu);
            await _context.SaveChangesAsync();
            return subMenu;
        }

        public async Task<bool> DeleteApp(Guid id)
        {
            var item = await _context.Applications.FindAsync(id);
            if (item == null) return false;
            _context.Applications.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMenu(Guid id)
        {
            var item = await _context.ApplicationMenus.FindAsync(id);
            if (item == null) return false;
            _context.ApplicationMenus.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSubMenu(Guid id)
        {
            var item = await _context.ApplicationSubMenus.FindAsync(id);
            if (item == null) return false;
            _context.ApplicationSubMenus.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        // Adicione os usings necessários se faltar

        public async Task<bool> SetPlanPermissions(PlanPermissionDto model)
        {
            // 1. Validar se o plano existe
            var plan = await _context.Plans.FindAsync(model.PlanId);
            if (plan == null) throw new Exception("Plano não encontrado");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // --- APPS ---
                // Remove tudo que existia antes para esse plano (estratégia "Limpar e Inserir" é a mais segura para sincronização)
                var oldApps = _context.PlanApplications.Where(x => x.PlanId == model.PlanId);
                _context.PlanApplications.RemoveRange(oldApps);
                
                // Adiciona os novos
                foreach (var appId in model.ApplicationIds)
                {
                    _context.PlanApplications.Add(new PlanApplication { PlanId = model.PlanId, ApplicationId = appId });
                }

                // --- MENUS ---
                var oldMenus = _context.PlanMenus.Where(x => x.PlanId == model.PlanId);
                _context.PlanMenus.RemoveRange(oldMenus);

                foreach (var menuId in model.MenuIds)
                {
                    _context.PlanMenus.Add(new PlanMenu { PlanId = model.PlanId, ApplicationMenuId = menuId });
                }

                // --- SUBMENUS ---
                var oldSubs = _context.PlanSubMenus.Where(x => x.PlanId == model.PlanId);
                _context.PlanSubMenus.RemoveRange(oldSubs);

                foreach (var subId in model.SubMenuIds)
                {
                    _context.PlanSubMenus.Add(new PlanSubMenu { PlanId = model.PlanId, ApplicationSubMenuId = subId });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}