using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECARTemplate.Models
{
    [Table("Empleados")]
    public class Empleado
    {
        [Key]
        public int Id { get; set; }

        [Column("CodigoEmpleado")]
        [   Required]
        [StringLength(50)]
        public string CodigoEmpleadoEcar { get; set; }

        [Column("Fecha")]
        [Required]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }

        [Column("NombreEmpleado")]
        [Required]
        [StringLength(255)]
        public string NombreEmpleado { get; set; }

        [Column("Cargo")]
        [Required]
        [StringLength(100)]
        public string Cargo { get; set; }

        [Column("Area")]
        [Required]
        [StringLength(100)]
        public string Area { get; set; }

        [Column("SubArea")]
        [Required]
        [StringLength(100)]
        public string SubArea { get; set; }

        [Column("Nota")]
        public string Nota { get; set; }

        [Column("Estado")]
        [Required]
        [StringLength(50)]
        public string Estado { get; set; }

        // Reemplaza el bloque actual de UsuarioRegistro con este:
        [Column("UsuarioRegistro")]
        [StringLength(255)]
        [Display(Name = "Usuario de Registro")]
        public string UsuarioRegistro { get; set; }

        [Column("FirmaBPM")]
        [Required]
        [StringLength(20)]
        public string FirmaBpm { get; set; }
    }
}
