namespace apiBit.DTOs
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UserDetailDto User {get; set;} = new();
        public List<AccessDto> Roles {get; set;} = new();
    }
}