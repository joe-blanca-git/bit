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

        public async Task<List<Plan>> GetAll()
        {
            return await _context.Plans.ToListAsync();
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