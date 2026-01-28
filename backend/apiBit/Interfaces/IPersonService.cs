using apiBit.DTOs;
using apiBit.Models;

namespace apiBit.Interfaces
{
    public interface IPersonService
    {
        Task<Person> CreateOrUpdateProfile(string userId, PersonDto model);

        Task<Person> GetProfileByUserId(string userId);
    } 
}