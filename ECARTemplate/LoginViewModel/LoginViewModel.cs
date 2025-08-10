using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ECARTemplate.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El campo Usuario es obligatorio.")]
        public string Usuario { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string ContrasenaUsuario { get; set; }

        public bool Recordarme { get; set; }

        // Agrega esta propiedad:
        [Display(Name = "Nombre de Usuario")] // Opcional, para la etiqueta en la vista
        public string NombreUsuario { get; set; }

        public IEnumerable<LoginViewModel> Items { get; set; }
        public string UsuarioActivo { get; set; }
    }
}
