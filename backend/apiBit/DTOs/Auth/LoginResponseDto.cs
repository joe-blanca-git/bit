using apiBit.DTOs; // <--- Importante

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public UserDetailDto User { get; set; }
    public List<AccessDto> Roles { get; set; } // O seu está retornando AccessDto no JSON
    public CompanyDto Company { get; set; } // O seu JSON tem Company

    // === VOCÊ PRECISA ADICIONAR ESTA LINHA ===
    public List<MenuDto> Menus { get; set; } = new(); 
}