namespace ECARTemplate.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Actividad
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha de registro es obligatoria.")]
        public DateTime FechaRegistro { get; set; }

        [Required(ErrorMessage = "El usuario de registro es obligatorio.")]
        [StringLength(50)]
        public string UsuarioRegistro { get; set; }

        [Required(ErrorMessage = "El código del equipo es obligatorio.")]
        [StringLength(50)]
        public string CodigoEquipo { get; set; }

        [Required(ErrorMessage = "El tipo de actividad es obligatorio.")]
        [StringLength(100)]
        public string TipoActividad { get; set; }

        public string Nota { get; set; }
    }
}