using apiBit.DTOs.Asaas;
using apiBit.Models;

namespace apiBit.Interfaces
{
    public interface IAsaasService
    {
        Task<string> CreateCustomer(User user, CreditCardHolderInfoDto holderInfo);
        
        Task<(string id, string status)> CreatePayment(string customerId, decimal value, CreditCardDto card, CreditCardHolderInfoDto holderInfo, string externalOrderId);

        Task<string> CreatePixCharge(string customerId, decimal value, string externalOrderId);
        
        Task<(string payload, string encodedImage)> GetPixQrCode(string asaasPaymentId);
    }
}