using apiBit.DTOs;
using apiBit.Models;

namespace apiBit.Interfaces
{
    public interface ICompanyService
    {
        // Retorna todas as empresas DO USUÁRIO
        Task<List<Company>> GetAll(string userId);
        
        // Busca uma específica (garantindo que é do usuário)
        Task<Company?> GetById(string userId, Guid companyId);
        
        // Cria empresa
        Task<Company> Create(string userId, CompanyDto model);
        
        // Deleta empresa
        Task<bool> Delete(string userId, Guid companyId);
    }
}