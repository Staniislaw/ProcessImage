using System.ComponentModel.DataAnnotations;

namespace ProcessImage.ViewModels
{
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Numele este obligatoriu")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Numele trebuie sa aiba între 2 si 100 caractere")]
        public string Nume { get; set; }

        [Required(ErrorMessage = "Email-ul este obligatoriu")]
        [EmailAddress(ErrorMessage = "Email-ul nu este valid")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Parola este obligatorie")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Parola trebuie sa aibacel putin 6 caractere")]
        [DataType(DataType.Password)]
        public string Parola { get; set; }

        [Required(ErrorMessage = "Confirmarea parolei este obligatorie")]
        [DataType(DataType.Password)]
        [Compare("Parola", ErrorMessage = "Parolele nu se potrivesc")]
        public string ConfirmaParola { get; set; }

        [Required(ErrorMessage = "Rolul este obligatoriu")]
        public long RolId { get; set; }

        [Required(ErrorMessage = "Subscriptia este obligatorie")]
        public long SubscriptieId { get; set; }
    }

}
