using Microsoft.AspNetCore.Mvc.Rendering;

using System.ComponentModel.DataAnnotations;

namespace ProcessImage.ViewModels
{
    public class CreateUserViewModel
    {
        public long? Id { get; set; } 

        [Required(ErrorMessage = "Numele este obligatoriu")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Numele trebuie sa aiba între 2 si 100 caractere")]
        public string Nume { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email-ul este obligatoriu")]
        [EmailAddress(ErrorMessage = "Email-ul nu este valid")]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        public string? Parola { get; set; }

        [DataType(DataType.Password)]
        [Compare("Parola", ErrorMessage = "Parolele nu se potrivesc")]
        public string? ConfirmaParola { get; set; }

        [Required(ErrorMessage = "Rolul este obligatoriu")]
        public long RolId { get; set; }
        public string RolNume { get; set; } = string.Empty;

        [Required(ErrorMessage = "Subscriptia este obligatorie")]
        public long SubscriptieId { get; set; }
        public string SubscriptieTip { get; set; } = string.Empty;
        public List<SelectListItem> Roluri { get; set; } = new();
        public List<SelectListItem> Subscriptii { get; set; } = new();
    }

}
