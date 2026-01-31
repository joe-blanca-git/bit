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

            // Configuração Básica do Cliente HTTP
            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Add("access_token", apiKey);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "BitSystem/1.0"); // Identificação para não ser bloqueado
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
                Console.WriteLine($"ERRO ASAAS CUSTOMER: {responseString}");
                throw new Exception($"Erro ao criar cliente no Asaas: {responseString}");
            }

            using var doc = JsonDocument.Parse(responseString);
            return doc.RootElement.GetProperty("id").GetString() ?? "";
        }

        // CORRIGIDO: Agora retorna ID e STATUS
        public async Task<(string id, string status)> CreatePayment(string customerId, decimal value, CreditCardDto card, CreditCardHolderInfoDto holderInfo, string externalOrderId)
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
                Console.WriteLine($"ERRO ASAAS PAGAMENTO: {responseString}");
                throw new Exception($"Erro no pagamento Asaas: {responseString}");
            }

            using var doc = JsonDocument.Parse(responseString);
            var root = doc.RootElement;

            if(root.TryGetProperty("id", out var idElement))
            {
                var id = idElement.GetString() ?? "";
                
                // Tenta pegar o status, se não vier, assume PENDING
                var status = "PENDING";
                if (root.TryGetProperty("status", out var statusElement))
                {
                    status = statusElement.GetString() ?? "PENDING";
                }

                return (id, status);
            }

            throw new Exception("ID do pagamento não retornado pelo Asaas.");
        }

        public async Task<string> CreatePixCharge(string customerId, decimal value, string externalOrderId)
        {
            var paymentData = new
            {
                customer = customerId,
                billingType = "PIX",
                value = value,
                dueDate = DateTime.Now.ToString("yyyy-MM-dd"),
                externalReference = externalOrderId
            };

            var content = new StringContent(JsonSerializer.Serialize(paymentData), Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("payments", content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"ERRO ASAAS PIX: {responseString}");
                throw new Exception($"Erro ao criar Pix: {responseString}");
            }

            using var doc = JsonDocument.Parse(responseString);
            return doc.RootElement.GetProperty("id").GetString() ?? "";
        }

        public async Task<(string payload, string encodedImage)> GetPixQrCode(string asaasPaymentId)
        {
            var response = await _httpClient.GetAsync($"payments/{asaasPaymentId}/pixQrCode");
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode) return ("", "");

            using var doc = JsonDocument.Parse(responseString);
            
            var payload = doc.RootElement.GetProperty("payload").GetString() ?? "";
            var encodedImage = doc.RootElement.GetProperty("encodedImage").GetString() ?? "";

            return (payload, encodedImage);
        }
    }
}