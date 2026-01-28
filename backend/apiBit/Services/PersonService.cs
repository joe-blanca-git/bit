using apiBit.Data;
using apiBit.DTOs;
using apiBit.Interfaces;
using apiBit.Models;
using Microsoft.EntityFrameworkCore;

namespace apiBit.Services
{
    public class PersonService : IPersonService
    {
        private readonly AppDbContext _context;

        public PersonService(AppDbContext context)
        {
            _context = context;
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
    
    }
}