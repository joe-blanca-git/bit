using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using apiBit.DTOs.Asaas;
using apiBit.Interfaces;
using apiBit.Models;

namespace apiBit.Services
{
    public class AsaasService : IAsaasService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AsaasService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            var baseUrl = _configuration["Asaas:BaseUrl"];
            var apiKey = _configuration["Asaas:ApiKey"];

            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Add("access_token", apiKey);
        }

        public async Task<string> CreateCustomer(User user, CreditCardHolderInfoDto holderInfo)
        {
            var customerData = new
            {
                name = holderInfo.Name,
                email = holderInfo.Email,
                cpfCnpj = holderInfo.CpfCnpj,
                postalCode = holderInfo.PostalCode,
                addressNumber = holderInfo.AddressNumber,
                phone = holderInfo.Phone,
                externalReference = user.Id 
            };

            var content = new StringContent(JsonSerializer.Serialize(customerData), Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("customers", content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Erro ao criar cliente no Asaas: {responseString}");
            }

            using var doc = JsonDocument.Parse(responseString);
            return doc.RootElement.GetProperty("id").GetString() ?? "";
        }

       // Adicione o parâmetro 'holderInfo' aqui na assinatura para bater com a Interface
        public async Task<string> CreatePayment(string customerId, decimal value, CreditCardDto card, CreditCardHolderInfoDto holderInfo, string externalOrderId)
        {
            var paymentData = new
            {
                customer = customerId,
                billingType = "CREDIT_CARD",
                value = value,
                dueDate = DateTime.Now.ToString("yyyy-MM-dd"),
                externalReference = externalOrderId,
                
                creditCard = new
                {
                    holderName = card.HolderName,
                    number = card.Number,
                    expiryMonth = card.ExpiryMonth,
                    expiryYear = card.ExpiryYear,
                    ccv = card.Ccv
                },
                
                // Usando os dados que chegaram no parâmetro novo
                creditCardHolderInfo = new
                {
                    name = holderInfo.Name,
                    email = holderInfo.Email,
                    cpfCnpj = holderInfo.CpfCnpj,
                    postalCode = holderInfo.PostalCode,
                    addressNumber = holderInfo.AddressNumber,
                    phone = holderInfo.Phone
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(paymentData), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("payments", content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // Dica: Esse log vai aparecer no terminal onde roda o 'dotnet run'
                Console.WriteLine($"ERRO ASAAS: {responseString}");
                throw new Exception($"Erro no pagamento Asaas: {responseString}");
            }

            using var doc = JsonDocument.Parse(responseString);
            
            // Tenta pegar o ID com segurança
            if(doc.RootElement.TryGetProperty("id", out var idElement))
            {
                return idElement.GetString() ?? "";
            }

            return "";
        }
    }
}