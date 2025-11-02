using System.ComponentModel.DataAnnotations;

namespace ProcessImage.ViewModels
{
    public class AdminLoginViewModel
    {
        [Required(ErrorMessage = "Email-ul este obligatoriu")]
        [EmailAddress(ErrorMessage = "Format email invalid")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Parola este obligatorie")]
        [DataType(DataType.Password)]
        public string Parola { get; set; }
        public bool RememberMe { get; set; }
    }

}
