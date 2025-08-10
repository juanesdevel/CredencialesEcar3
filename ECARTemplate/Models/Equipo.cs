using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECARTemplate.Models
{
    public class Equipo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("CodigoEquipo")]
        [Required(ErrorMessage = "El código de equipo es obligatorio.")]
        [MaxLength(50)]
        [Display(Name = "Código del Equipo")]
        public string CodigoEquipo { get; set; }

        [Column("Fecha")]
        [Display(Name = "Fecha de Creación")]
        public DateTime? Fecha { get; set; }

        [Column("NombreEquipo")]
        [Required(ErrorMessage = "Nombre de equipo obligatorio.")]
        [MaxLength(100)]
        [Display(Name = "Nombre del Equipo")]
        public string NombreEquipo { get; set; }

        [Column("Sede")]
        [Required(ErrorMessage = "La Sede es obligatoria.")]
        [MaxLength(50)]
        [Display(Name = "Sede")]
        public string Sede { get; set; }

        [Column("Area")]
        [Required(ErrorMessage = "El área es obligatoria.")]
        [MaxLength(50)]
        [Display(Name = "Área")]
        public string Area { get; set; }

        [Column("SubArea")]
        [Required(ErrorMessage = "La Sub Área es obligatoria.")]
        [MaxLength(50)]
        [Display(Name = "Sub Área")]
        public string SubArea { get; set; }

        [Column("Nota")]
        [Display(Name = "Nota")]
        public string Nota { get; set; }

        [Column("Estado")]
        [MaxLength(50)]
        [Display(Name = "Estado")]
        public string Estado { get; set; }

        // Reemplaza el bloque actual de UsuarioTiRegistro con este:
        [Column("UsuarioRegistro")]
        [StringLength(255)]
        [Display(Name = "Usuario de Registro")]
        public string UsuarioRegistro { get; set; }

        [Column("RutaImagen")]
        [MaxLength(500)]
        [Display(Name = "Ruta de la Imagen")]
        public string RutaImagen { get; set; }
  
        [Column("HojaDeVida")]
        [MaxLength(2048)] 
        [Display(Name = "Hoja de Vida / Link")] 
        public string HojaDeVida { get; set; }

    }
}