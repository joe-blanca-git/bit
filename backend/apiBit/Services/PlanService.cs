using apiBit.Data;
using apiBit.DTOs;
using apiBit.Interfaces;
using apiBit.Models;
using Microsoft.EntityFrameworkCore;

namespace apiBit.Services
{
    public class PlanService : IPlanService
    {
        private readonly AppDbContext _context;

        public PlanService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<PlanResponseDto>> GetAll()
        {
            // 1. Busca os dados brutos do banco (com os Includes necessários)
            var plans = await _context.Plans
                .Include(p => p.AllowedApps)
                    .ThenInclude(pa => pa.Application)
                .Include(p => p.AllowedMenus)
                    .ThenInclude(pm => pm.ApplicationMenu)
                .Include(p => p.AllowedSubMenus)
                    .ThenInclude(ps => ps.ApplicationSubMenu)
                .ToListAsync();

            // 2. Monta a Árvore (Mapeamento)
            var result = plans.Select(plan => new PlanResponseDto
            {
                Id = plan.Id,
                Name = plan.Name,
                Description = plan.Description,
                Price = plan.Price,
                DiscountPrice = plan.DiscountPrice,
                
                // AQUI ESTA A LOGICA DA ARVORE
                AllowedApps = plan.AllowedApps.Select(app => new PlanAppTreeDto
                {
                    Id = app.Id,
                    PlanId = app.PlanId,
                    ApplicationId = app.ApplicationId,
                    ApplicationName = app.Application?.Name ?? "",

                    // Dentro deste App, filtro apenas os Menus que pertencem a ele E que estão permitidos no plano
                    AllowedMenus = plan.AllowedMenus
                        .Where(menu => menu.ApplicationMenu!.ApplicationId == app.ApplicationId)
                        .Select(menu => new PlanMenuTreeDto
                        {
                            ApplicationMenuId = menu.ApplicationMenuId,
                            Title = menu.ApplicationMenu?.Title ?? "",

                            // Dentro deste Menu, filtro apenas os SubMenus que pertencem a ele E que estão permitidos
                            AllowedSubMenus = plan.AllowedSubMenus
                                .Where(sub => sub.ApplicationSubMenu!.ApplicationMenuId == menu.ApplicationMenuId)
                                .Select(sub => new PlanSubMenuTreeDto
                                {
                                    ApplicationSubMenuId = sub.ApplicationSubMenuId,
                                    Title = sub.ApplicationSubMenu?.Title ?? ""
                                }).ToList()

                        }).ToList()

                }).ToList()
            }).ToList();

            return result;
        }

        public async Task<Plan?> GetById(Guid id)
        {
            return await _context.Plans.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Plan> Create(PlanDto model)
        {
            var plan = new Plan
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                DiscountPrice = model.DiscountPrice
            };

            await _context.Plans.AddAsync(plan);
            await _context.SaveChangesAsync();
            return plan;
        }

        public async Task<Plan?> Update(Guid id, PlanDto model)
        {
            var plan = await _context.Plans.FirstOrDefaultAsync(p => p.Id == id);
            if (plan == null) return null;

            plan.Name = model.Name;
            plan.Description = model.Description;
            plan.Price = model.Price;
            plan.DiscountPrice = model.DiscountPrice;

            await _context.SaveChangesAsync();
            return plan;
        }

        public async Task<bool> Delete(Guid id)
        {
            var plan = await _context.Plans.FirstOrDefaultAsync(p => p.Id == id);
            if (plan == null) return false;

            _context.Plans.Remove(plan);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}