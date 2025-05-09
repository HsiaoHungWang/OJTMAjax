using System.ComponentModel.DataAnnotations;

namespace MSITApp.Models.DTO
{
    public class LoginDTO
    {   
        public string? Email { get; set; }
      
        public string? Password { get; set; }
    }
}
