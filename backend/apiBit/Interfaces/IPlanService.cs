using apiBit.DTOs;
using apiBit.Models;

namespace apiBit.Interfaces
{
    public interface IPlanService
    {
        Task<List<PlanResponseDto>> GetAll();
        Task<Plan?> GetById(Guid id);
        Task<Plan> Create(PlanDto model);
        Task<Plan?> Update(Guid id, PlanDto model);
        Task<bool> Delete(Guid id);
    }
}