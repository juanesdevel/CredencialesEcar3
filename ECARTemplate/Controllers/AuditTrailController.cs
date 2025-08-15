// Controllers/AuditTrailController.cs
using ECARTemplate.Data;
using ECARTemplate.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ECARTemplate.Controllers
{
    [Authorize]
    public class AuditTrailController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditTrailController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            DateTime? fechaDesdeFiltro,
            DateTime? fechaHastaFiltro,
            string usuarioFiltro,
            string tipoAccionFiltro,
            string moduloFiltro,
            string sortOrder = "")
        {
            // Parámetros de ordenamiento para la vista
            ViewData["FechaSortParam"] = sortOrder == "Fecha" ? "fecha_desc" : "Fecha";
            ViewData["UsuarioSortParam"] = sortOrder == "Usuario" ? "usuario_desc" : "Usuario";
            ViewData["TipoAccionSortParam"] = sortOrder == "TipoAccion" ? "tipoaccion_desc" : "TipoAccion";
            ViewData["ModuloSortParam"] = sortOrder == "Modulo" ? "modulo_desc" : "Modulo";

            var auditTrails = _context.AuditTrails.AsQueryable();

            // Filtros
            if (fechaDesdeFiltro.HasValue)
            {
                auditTrails = auditTrails.Where(a => a.FechaRegistro >= fechaDesdeFiltro.Value);
            }
            if (fechaHastaFiltro.HasValue)
            {
                auditTrails = auditTrails.Where(a => a.FechaRegistro < fechaHastaFiltro.Value.AddDays(1));
            }
            if (!string.IsNullOrEmpty(usuarioFiltro))
            {
                auditTrails = auditTrails.Where(a => a.Usuario.Contains(usuarioFiltro));
            }
            if (!string.IsNullOrEmpty(tipoAccionFiltro))
            {
                auditTrails = auditTrails.Where(a => a.TipoAccion == tipoAccionFiltro);
            }
            if (!string.IsNullOrEmpty(moduloFiltro))
            {
                auditTrails = auditTrails.Where(a => a.Modulo == moduloFiltro);
            }

            // Ordenamiento
            switch (sortOrder)
            {
                case "fecha_desc":
                    auditTrails = auditTrails.OrderByDescending(a => a.FechaRegistro);
                    break;
                case "Usuario":
                    auditTrails = auditTrails.OrderBy(a => a.Usuario);
                    break;
                case "usuario_desc":
                    auditTrails = auditTrails.OrderByDescending(a => a.Usuario);
                    break;
                case "TipoAccion":
                    auditTrails = auditTrails.OrderBy(a => a.TipoAccion);
                    break;
                case "tipoaccion_desc":
                    auditTrails = auditTrails.OrderByDescending(a => a.TipoAccion);
                    break;
                case "Modulo":
                    auditTrails = auditTrails.OrderBy(a => a.Modulo);
                    break;
                case "modulo_desc":
                    auditTrails = auditTrails.OrderByDescending(a => a.Modulo);
                    break;
                default:
                    auditTrails = auditTrails.OrderByDescending(a => a.FechaRegistro);
                    break;
            }

            var auditTrailsList = await auditTrails.ToListAsync();

            // Guardar los filtros para que la vista los mantenga
            ViewData["FechaDesdeFiltro"] = fechaDesdeFiltro?.ToString("yyyy-MM-dd");
            ViewData["FechaHastaFiltro"] = fechaHastaFiltro?.ToString("yyyy-MM-dd");
            ViewData["UsuarioFiltro"] = usuarioFiltro;
            ViewData["TipoAccionFiltro"] = tipoAccionFiltro;
            ViewData["ModuloFiltro"] = moduloFiltro;
            ViewData["TotalRegistros"] = auditTrailsList.Count;

            return View(auditTrailsList);
        }
    }
}