using apiBit.DTOs;
using apiBit.Models;

namespace apiBit.Interfaces
{
    public interface IPersonService
    {
        Task<Person> CreateOrUpdateProfile(string userId, PersonDto model);

        Task<Person?> GetProfileByUserId(string userId);

        /// <summary>
        /// Retorna lista de pessoas. Se userType for informado, filtra pela Role.
        /// </summary>
        Task<List<Person>> GetAllProfiles(string? userType = null);
    } 
}