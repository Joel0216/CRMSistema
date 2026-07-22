using System.ComponentModel.DataAnnotations;

namespace CRMSistema.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "El usuario contiene caracteres no permitidos. Use solo letras, números y guiones bajos.")]
        [Display(Name = "Usuario")]
        public string Usuario { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Display(Name = "Recordarme")]
        public bool Recordarme { get; set; }

        public string Error { get; set; }
    }
}
