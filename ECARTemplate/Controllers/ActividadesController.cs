namespace ECARTemplate.Controllers
{
    using ECARTemplate.Data;
    using ECARTemplate.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Data.SqlClient; // Asegúrate de tener este 'using'

    [Authorize]
    public class ActividadesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActividadesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Actividades
        public async Task<IActionResult> Index(
            DateTime? fechaDesdeFiltro,
            DateTime? fechaHastaFiltro,
            string usuarioRegistroFiltro,
            string codigoEquipoFiltro,
            string tipoActividadFiltro,
            string notaFiltro,
            string sortOrder = "")
        {
            // Parámetros de ordenamiento para la vista
            ViewData["FechaSortParam"] = sortOrder == "Fecha" ? "fecha_desc" : "Fecha";
            ViewData["UsuarioSortParam"] = sortOrder == "Usuario" ? "usuario_desc" : "Usuario";
            ViewData["CodigoEquipoSortParam"] = sortOrder == "CodigoEquipo" ? "codigoequipo_desc" : "CodigoEquipo";
            ViewData["TipoActividadSortParam"] = sortOrder == "TipoActividad" ? "tipoactividad_desc" : "TipoActividad";
            ViewData["NotaSortParam"] = sortOrder == "Nota" ? "nota_desc" : "Nota";

            var actividades = _context.Actividades.AsQueryable();

            // Filtros
            if (fechaDesdeFiltro.HasValue)
            {
                actividades = actividades.Where(a => a.FechaRegistro >= fechaDesdeFiltro.Value);
            }
            if (fechaHastaFiltro.HasValue)
            {
                // Se suma un día para incluir todos los registros de la fecha final.
                actividades = actividades.Where(a => a.FechaRegistro < fechaHastaFiltro.Value.AddDays(1));
            }
            if (!string.IsNullOrEmpty(usuarioRegistroFiltro))
            {
                actividades = actividades.Where(a => a.UsuarioRegistro.Contains(usuarioRegistroFiltro));
            }
            if (!string.IsNullOrEmpty(codigoEquipoFiltro))
            {
                actividades = actividades.Where(a => a.CodigoEquipo.Contains(codigoEquipoFiltro));
            }
            if (!string.IsNullOrEmpty(tipoActividadFiltro))
            {
                actividades = actividades.Where(a => a.TipoActividad == tipoActividadFiltro);
            }
            if (!string.IsNullOrEmpty(notaFiltro))
            {
                actividades = actividades.Where(a => a.Nota.Contains(notaFiltro));
            }

            // Ordenamiento
            switch (sortOrder)
            {
                case "fecha_desc":
                    actividades = actividades.OrderByDescending(a => a.FechaRegistro);
                    break;
                case "Usuario":
                    actividades = actividades.OrderBy(a => a.UsuarioRegistro);
                    break;
                case "usuario_desc":
                    actividades = actividades.OrderByDescending(a => a.UsuarioRegistro);
                    break;
                case "CodigoEquipo":
                    actividades = actividades.OrderBy(a => a.CodigoEquipo);
                    break;
                case "codigoequipo_desc":
                    actividades = actividades.OrderByDescending(a => a.CodigoEquipo);
                    break;
                case "TipoActividad":
                    actividades = actividades.OrderBy(a => a.TipoActividad);
                    break;
                case "tipoactividad_desc":
                    actividades = actividades.OrderByDescending(a => a.TipoActividad);
                    break;
                case "Nota":
                    actividades = actividades.OrderBy(a => a.Nota);
                    break;
                case "nota_desc":
                    actividades = actividades.OrderByDescending(a => a.Nota);
                    break;
                default:
                    actividades = actividades.OrderByDescending(a => a.FechaRegistro); // Por defecto ordenar por fecha descendente
                    break;
            }

            var actividadesList = await actividades.ToListAsync();

            // Guardar los filtros en ViewBag para que la vista pueda mantener los valores
            ViewBag.FechaDesdeFiltro = fechaDesdeFiltro;
            ViewBag.FechaHastaFiltro = fechaHastaFiltro;
            ViewBag.UsuarioRegistroFiltro = usuarioRegistroFiltro;
            ViewBag.CodigoEquipoFiltro = codigoEquipoFiltro;
            ViewBag.TipoActividadFiltro = tipoActividadFiltro;
            ViewBag.NotaFiltro = notaFiltro;
            ViewData["TotalRegistros"] = actividadesList.Count;

            return View(actividadesList);
        }

        // GET: /Actividades/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Actividades/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Actividad actividad)
        {
            if (ModelState.IsValid)
            {
                actividad.FechaRegistro = DateTime.Now;
                actividad.UsuarioRegistro = User.Identity.Name;

                _context.Actividades.Add(actividad);
                await _context.SaveChangesAsync();

                // Lógica de Auditoría: Registro de creación
                await RegistrarAuditoriaAsync(User.Identity.Name, "Crear", "Actividades", $"Se creó una actividad para el equipo '{actividad.CodigoEquipo}' con tipo '{actividad.TipoActividad}'.");

                return RedirectToAction(nameof(Index));
            }
            return View(actividad);
        }

        // Acción GET para buscar un equipo por su código (llamada desde AJAX)
        [HttpGet]
        public async Task<IActionResult> ObtenerDatosEquipo(string codigoEquipo)
        {
            if (string.IsNullOrEmpty(codigoEquipo))
            {
                return Json(new { success = false, message = "El código de equipo no puede estar vacío." });
            }

            try
            {
                // Busca el equipo de forma asíncrona
                var equipo = await _context.Equipos.FirstOrDefaultAsync(e => e.CodigoEquipo.ToUpper() == codigoEquipo.ToUpper());

                if (equipo != null)
                {
                    // Retorna un objeto JSON con éxito y los datos del equipo
                    return Json(new { success = true, data = new { codigoEquipo = equipo.CodigoEquipo, nombreEquipo = equipo.NombreEquipo } });
                }
                else
                {
                    // Retorna un objeto JSON con un mensaje de error si no se encuentra el equipo
                    return Json(new { success = false, message = "No se encontró ningún equipo con ese código." });
                }
            }
            catch (Exception ex)
            {
                // Retorna un objeto JSON con un mensaje de error en caso de excepción
                return Json(new { success = false, message = "Ocurrió un error interno al buscar el equipo." });
            }
        }

        // GET: Actividades/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actividad = await _context.Actividades.FindAsync(id);
            if (actividad == null)
            {
                return NotFound();
            }

            return View(actividad);
        }

        // POST: Actividades/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FechaRegistro,UsuarioRegistro,TipoActividad,CodigoEquipo,Nota")] Actividad actividad)
        {
            if (id != actividad.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Lógica de Auditoría: Se captura la actividad original para el detalle.
                    var actividadOriginal = await _context.Actividades.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
                    if (actividadOriginal != null)
                    {
                        string detalle = $"Se editó la actividad con ID {id}. Cambios: TipoActividad de '{actividadOriginal.TipoActividad}' a '{actividad.TipoActividad}', Nota de '{actividadOriginal.Nota}' a '{actividad.Nota}'.";
                        await RegistrarAuditoriaAsync(User.Identity.Name, "Editar", "Actividades", detalle);
                    }

                    _context.Update(actividad);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActividadExists(actividad.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(actividad);
        }

        // GET: Actividades/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actividad = await _context.Actividades
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actividad == null)
            {
                return NotFound();
            }

            return View(actividad);
        }

        // POST: Actividades/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actividad = await _context.Actividades.FindAsync(id);
            _context.Actividades.Remove(actividad);
            await _context.SaveChangesAsync();

            // Lógica de Auditoría: Registro de eliminación
            await RegistrarAuditoriaAsync(User.Identity.Name, "Eliminar", "Actividades", $"Se eliminó la actividad con ID {id} para el equipo '{actividad.CodigoEquipo}'.");

            return RedirectToAction(nameof(Index));
        }

        private bool ActividadExists(int id)
        {
            return _context.Actividades.Any(e => e.Id == id);
        }

        /// <summary>
        /// Método auxiliar para registrar la auditoría.
        /// </summary>
        private async Task RegistrarAuditoriaAsync(string usuario, string tipoAccion, string modulo, string detalle)
        {
            var parameters = new[]
            {
                new SqlParameter("@Usuario", usuario),
                new SqlParameter("@TipoAccion", tipoAccion),
                new SqlParameter("@Modulo", modulo),
                new SqlParameter("@DetalleCambio", detalle)
            };
            await _context.Database.ExecuteSqlRawAsync("EXEC sp_InsertarAuditTrail @Usuario, @TipoAccion, @Modulo, @DetalleCambio", parameters);
        }
    }
}