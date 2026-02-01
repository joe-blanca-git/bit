namespace apiBit.DTOs
{
    public class FinancialTransactionFilterDto
    {
        // Filtro Genérico (Busca em tudo)
        public string? Q { get; set; } 

        // Filtros de Data (Pelo DocumentDate)
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Filtros Específicos
        public Guid? PersonId { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? OriginId { get; set; }
        public Guid? AccountId { get; set; }
        
        public int? Type { get; set; } // 1 = Receita, 2 = Despesa

        // Paginação (Padrão: Página 1, 20 itens)
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}