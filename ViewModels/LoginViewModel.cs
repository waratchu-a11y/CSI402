using System.ComponentModel.DataAnnotations;

namespace warat_0583_l005.ViewModels
{
    public class LoginViewModel
    {
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}