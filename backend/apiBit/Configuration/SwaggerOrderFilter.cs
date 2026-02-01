using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace apiBit.Configuration
{
    public class SwaggerOrderFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {

            swaggerDoc.Tags = new List<OpenApiTag>
            {
                new OpenApiTag { Name = "Auth", Description = "Login e Registro de Usuários" },
                new OpenApiTag { Name = "Person", Description = "Dados Pessoais do Usuário" },
                new OpenApiTag { Name = "Address", Description = "Gerenciamento de Endereços" },
                new OpenApiTag { Name = "Company", Description = "Gerenciamento de Empresas" },
                new OpenApiTag { Name = "Plan", Description = "Gerenciamento de Panos (Restrito para Adminsitradores)" },
                new OpenApiTag { Name = "AppManager", Description = "Gerenciamento de Aplicativos (Restrito para Desenvolvedores)" },
                new OpenApiTag { Name = "Checkout", Description = "Api de pagamento integrada com banco Asaas" },
                new OpenApiTag { Name = "FinancialAccount", Description = "Contas Financeiras. Ex: Bancos, Carteiras e Corretoras" },
                new OpenApiTag { Name = "FinancialOrigin", Description = "Origem das movimentações financeiras. Ex: Pedido de Venda, Venda no E-commerce, Lançamento Manual..." },
                new OpenApiTag { Name = "FinancialCategory", Description = "Categorias das movimentações financeiras. Ex: Salário, Aluguel, Energia, Supermercado..." },
                new OpenApiTag { Name = "FinancialTransaction", Description = "Movimentações Financeiras" }
            };
        }
    }
}