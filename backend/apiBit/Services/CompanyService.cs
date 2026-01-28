using apiBit.Data;
using apiBit.DTOs;
using apiBit.Interfaces;
using apiBit.Models;
using Microsoft.EntityFrameworkCore;

namespace apiBit.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly AppDbContext _context;

        public CompanyService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Company>> GetAll(string userId)
        {
            return await _context.Companies
                .Include(c => c.Addresses) // Traz os endereços junto
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        public async Task<Company?> GetById(string userId, Guid companyId)
        {
            return await _context.Companies
                .Include(c => c.Addresses)
                .FirstOrDefaultAsync(c => c.Id == companyId && c.UserId == userId);
        }

        public async Task<Company> Create(string userId, CompanyDto model)
        {
            // Opcional: Verificar se CNPJ já existe no banco todo
            // if (await _context.Companies.AnyAsync(c => c.Document == model.Document)) 
            //    throw new Exception("CNPJ já cadastrado.");

            var company = new Company
            {
                UserId = userId,
                Name = model.Name,
                Document = model.Document,
                Activity = model.Activity,
                Addresses = new List<CompanyAddress>()
            };

            // Adiciona endereços
            foreach (var addr in model.Addresses)
            {
                company.Addresses.Add(new CompanyAddress
                {
                    ZipCode = addr.ZipCode, Street = addr.Street, 
                    Number = addr.Number, Complement = addr.Complement,
                    City = addr.City, Neighborhood = addr.Neighborhood, 
                    State = addr.State
                });
            }

            await _context.Companies.AddAsync(company);
            await _context.SaveChangesAsync();
            return company;
        }

        public async Task<bool> Delete(string userId, Guid companyId)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == companyId && c.UserId == userId);

            if (company == null) return false;

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}