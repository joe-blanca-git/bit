namespace apiBit.DTOs
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        
        // Se no futuro você retornar o Nome ou Role aqui, é só adicionar as propriedades
        // public string UserName { get; set; } 
    }
}