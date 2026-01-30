using apiBit.DTOs.Asaas;
using apiBit.Models;

namespace apiBit.Interfaces
{
    public interface IAsaasService
    {
        Task<string> CreateCustomer(User user, CreditCardHolderInfoDto holderInfo);

        Task<string> CreatePayment(string customerId, decimal value, CreditCardDto card, CreditCardHolderInfoDto holderInfo, string externalOrderId);
    }
}