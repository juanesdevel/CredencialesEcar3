using System.ComponentModel.DataAnnotations;

namespace ECARTemplate.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string NombreUsuario { get; set; }

        [Required(ErrorMessage = "El usuario es obligatorio")]
        public string Usuario { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string ContrasenaUsuario { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio")]
        public string RolUsuario { get; set; }
    }
}