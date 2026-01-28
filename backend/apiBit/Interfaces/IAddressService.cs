using apiBit.DTOs;
using apiBit.Models;

namespace apiBit.Interfaces
{
    public interface IAddressService
    {
        // Lista todos os endereços do usuário logado
        Task<List<PersonAddress>> GetAll(string userId);

        // Pega um endereço específico (para edição, por exemplo)
        Task<PersonAddress?> GetById(string userId, Guid addressId);

        // Adiciona um novo endereço
        Task<PersonAddress> Add(string userId, AddressDto model);

        // Atualiza um endereço existente
        Task<PersonAddress?> Update(string userId, Guid addressId, AddressDto model);

        // Deleta um endereço
        Task<bool> Delete(string userId, Guid addressId);
    }
}