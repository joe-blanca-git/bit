using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace apiBit.Configuration
{
    public class SwaggerOrderFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // Aqui definimos a ordem exata que queremos
            // O Swagger vai respeitar a ordem desta lista
            swaggerDoc.Tags = new List<OpenApiTag>
            {
                new OpenApiTag { Name = "Auth", Description = "Login e Registro de Usuários" },
                new OpenApiTag { Name = "Person", Description = "Dados Pessoais do Usuário" },
                new OpenApiTag { Name = "Address", Description = "Gerenciamento de Endereços" }
            };
        }
    }
}