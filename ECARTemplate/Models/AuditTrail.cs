// Models/AuditTrail.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECARTemplate.Models
{
    public class AuditTrail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime FechaRegistro { get; set; }

        [Required]
        [MaxLength(256)]
        public string Usuario { get; set; }

        [Required]
        [MaxLength(50)]
        public string TipoAccion { get; set; } // Por ejemplo: "Crear", "Editar", "Eliminar", "Inactivar"

        [Required]
        [MaxLength(50)]
        public string Modulo { get; set; } // Por ejemplo: "Usuarios", "Equipos", "Credenciales"

        public string DetalleCambio { get; set; } // JSON o texto con los cambios realizados (opcional)
    }
}