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
        // This method is needed to display the form.
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Actividades/Create
        // Acción para procesar la creación de una nueva actividad.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Actividad actividad)
        {
            if (ModelState.IsValid)
            {
                actividad.FechaRegistro = DateTime.Now;
                actividad.UsuarioRegistro = User.Identity.Name;

                _context.Actividades.Add(actividad);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(actividad);
        }
    }
}