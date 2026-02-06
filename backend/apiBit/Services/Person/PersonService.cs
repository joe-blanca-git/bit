using apiBit.Data;
using apiBit.DTOs;
using apiBit.Interfaces;
using apiBit.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace apiBit.Services
{
    public class PersonService : IPersonService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public PersonService(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<Person?> GetProfileByUserId(string userId)
        {
            return await _context.People
                .Include(p => p.Addresses)
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<Person> CreateOrUpdateProfile(string userId, PersonDto model)
        {
            var person = await _context.People
                .Include(p => p.Addresses)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (person == null)
            {
                person = new Person
                {
                    UserId = userId, 
                    Addresses = new List<PersonAddress>()
                };
                await _context.People.AddAsync(person);
            }

            person.Name = model.Name;
            person.Document = model.Document;
            person.BirthDate = model.BirthDate;
            person.Phone = model.Phone;
            person.PhoneSecondary = model.PhoneSecondary;
            person.EmailSecondary = model.EmailSecondary;
            person.Position = model.Position;

            person.Addresses.Clear();

            foreach (var addrDto in model.Addresses)
            {
                person.Addresses.Add(new PersonAddress
                {
                    ZipCode = addrDto.ZipCode,
                    Street = addrDto.Street,
                    Number = addrDto.Number,
                    Complement = addrDto.Complement,
                    City = addrDto.City,
                    Neighborhood = addrDto.Neighborhood
                });
            }

            await _context.SaveChangesAsync();
            return person;
        }

        public async Task<List<Person>> GetAllProfiles(string? userType = null)
        {
            var query = _context.People
                                .AsNoTracking()
                                .Include(p => p.Addresses) // Traz os endereços juntos
                                .AsQueryable();

            // Se não passar tipo, retorna todo mundo
            if (string.IsNullOrWhiteSpace(userType))
            {
                return await query.OrderBy(p => p.Name).ToListAsync();
            }

            // Se passar tipo (Ex: CLIENT), precisamos achar os usuários dessa role
            try 
            {
                // 1. Busca usuários na Role (Ex: todos os Users que são CLIENT)
                // O GetUsersInRoleAsync é do Identity e facilita muito nossa vida
                var usersInRole = await _userManager.GetUsersInRoleAsync(userType.ToUpper());
                
                if (usersInRole == null || !usersInRole.Any())
                {
                    return new List<Person>(); // Ninguém nessa role
                }

                // 2. Extrai os IDs desses usuários
                var userIds = usersInRole.Select(u => u.Id).ToList();

                // 3. Filtra a tabela People onde o UserId esteja na lista
                query = query.Where(p => userIds.Contains(p.UserId));

                return await query.OrderBy(p => p.Name).ToListAsync();
            }
            catch (Exception)
            {
                // Se a role não existir (ex: passaram "BATMAN"), o Identity pode lançar erro
                // ou retornar vazio. Por segurança, retornamos lista vazia.
                return new List<Person>();
            }
        }
    
    }
}