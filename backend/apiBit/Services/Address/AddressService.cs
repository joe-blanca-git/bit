using apiBit.Data;
using apiBit.DTOs;
using apiBit.Interfaces;
using apiBit.Models;
using Microsoft.EntityFrameworkCore;

namespace apiBit.Services
{
    public class AddressService : IAddressService
    {
        private readonly AppDbContext _context;

        public AddressService(AppDbContext context)
        {
            _context = context;
        }

        // Método auxiliar para achar o ID da Pessoa baseado no Usuário logado
        private async Task<Person?> GetPersonByUserId(string userId)
        {
            return await _context.People.FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<List<PersonAddress>> GetAll(string userId)
        {
            var person = await GetPersonByUserId(userId);
            if (person == null) return new List<PersonAddress>();

            return await _context.PersonAddresses
                .Where(a => a.PersonId == person.Id)
                .ToListAsync();
        }

        public async Task<PersonAddress?> GetById(string userId, Guid addressId)
        {
            var person = await GetPersonByUserId(userId);
            if (person == null) return null;

            return await _context.PersonAddresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.PersonId == person.Id);
        }

        public async Task<PersonAddress> Add(string userId, AddressDto model)
        {
            var person = await GetPersonByUserId(userId);
            if (person == null) throw new Exception("Perfil não encontrado. Crie seu perfil antes de adicionar endereços.");

            var newAddress = new PersonAddress
            {
                PersonId = person.Id, // Vincula à pessoa correta
                ZipCode = model.ZipCode,
                Street = model.Street,
                Number = model.Number,
                Complement = model.Complement,
                City = model.City,
                Neighborhood = model.Neighborhood,
                State = model.State
            };

            await _context.PersonAddresses.AddAsync(newAddress);
            await _context.SaveChangesAsync();
            return newAddress;
        }

        public async Task<PersonAddress?> Update(string userId, Guid addressId, AddressDto model)
        {
            var person = await GetPersonByUserId(userId);
            if (person == null) return null;

            // Busca o endereço garantindo que ele pertence a essa pessoa
            var address = await _context.PersonAddresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.PersonId == person.Id);

            if (address == null) return null; // Endereço não existe ou não é desse usuário

            // Atualiza os campos
            address.ZipCode = model.ZipCode;
            address.Street = model.Street;
            address.Number = model.Number;
            address.Complement = model.Complement;
            address.City = model.City;
            address.Neighborhood = model.Neighborhood;
            address.State = model.State;

            await _context.SaveChangesAsync();
            return address;
        }

        public async Task<bool> Delete(string userId, Guid addressId)
        {
            var person = await GetPersonByUserId(userId);
            if (person == null) return false;

            var address = await _context.PersonAddresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.PersonId == person.Id);

            if (address == null) return false;

            _context.PersonAddresses.Remove(address);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}