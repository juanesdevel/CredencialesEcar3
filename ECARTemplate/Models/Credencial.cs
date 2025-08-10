using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECARTemplate.Models
{
    [Table("Credenciales")]
    public class Credencial
    {
        [Key]
        public int Id { get; set; }

        [Column("CodigoEquipo")]
        [Required(ErrorMessage = "El código de equipo es obligatorio.")]
        [StringLength(50)]
        [Display(Name = "Código del Equipo")]

        public string CodigoEquipo { get; set; }

        [Column("Fecha")]
        [Required(ErrorMessage = "La fecha y hora son obligatorias.")]
        [Display(Name = "Fecha de Creación")]

        public DateTime FechaYHora { get; set; }

        [Column("CodigoUsuario")]
        [Required(ErrorMessage = "El código de usuario es obligatorio.")]
        [StringLength(50)]
        [Display(Name = "Código de Empleado")]

        public string CodigoUsuarioEcar { get; set; }

        [Column("NombreEmpleado")]
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(255)]
        [Display(Name = "Nombre de Empleado")]

        public string NombreUsuario { get; set; }

        [Column("Perfil")]
        [Required(ErrorMessage = "El perfil es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Perfil")]

        public string Perfil { get; set; }

        [Column("Usuario")]
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Usuario")]

        public string Usuario { get; set; }

        [Column("Contrasena")]
        [StringLength(255)]
        [Display(Name = "Contraseña")]

        public string Contrasena { get; set; }

        [Column("Estado")]
        [Required(ErrorMessage = "El estado es obligatorio.")]
        [StringLength(50)]
        [Display(Name = "Estado")]

        public string Estado { get; set; }

        // Reemplaza el bloque actual de UsuarioRegistro con este:
        [Column("UsuarioRegistro")]
        [StringLength(255)]
        [Display(Name = "Usuario de Registro")]
        public string UsuarioRegistro { get; set; }
    }
}