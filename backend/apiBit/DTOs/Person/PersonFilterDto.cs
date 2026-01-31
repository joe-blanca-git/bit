using System.ComponentModel.DataAnnotations;


namespace apiBit.DTOs
{
    public class PersonFilterDto
    {
        public string? Name { get; set; }
        public string? Document { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
    }
}