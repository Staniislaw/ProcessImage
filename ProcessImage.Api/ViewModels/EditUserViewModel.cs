using System.ComponentModel.DataAnnotations;

namespace ProcessImage.ViewModels
{
    public class EditUserViewModel
    {
        public long Id { get; set; }
        [Required(ErrorMessage = "Numele este obligatoriu")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Numele trebuie sa aiba între 2 si 100 caractere")]
        public string Nume { get; set; }
        [Required(ErrorMessage = "Email-ul este obligatoriu")]
        [EmailAddress(ErrorMessage = "Email-ul nu este valid")]
        public string Email { get; set; }
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Parola trebuie sa aibacel putin 6 caractere")]
        [DataType(DataType.Password)]
        public string? NovaParola { get; set; }
        [DataType(DataType.Password)]
        [Compare("NovaParola", ErrorMessage = "Parolele nu se potrivesc")]
        public string? ConfirmaNovaParola { get; set; }
        [Required(ErrorMessage = "Rolul este obligatoriu")]
        public long RolId { get; set; }
    }


}
