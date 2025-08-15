using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using ECARTemplate.Models;
using ECARTemplate.Data;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;

namespace ECARTemplate.Controllers
{
    [Authorize]
    public class EmpleadosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmpleadosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerDatosUsuario(string codigoEmpleadoEcar)
        {
            try
            {
                Console.WriteLine($"Buscando empleado con código: {codigoEmpleadoEcar}");

                if (string.IsNullOrEmpty(codigoEmpleadoEcar))
                {
                    return Json(new { success = false, message = "El Código de Empleado es requerido." });
                }

                var empleado = await _context.Empleados
                    .Where(e => e.CodigoEmpleadoEcar == codigoEmpleadoEcar)
                    .Select(e => new
                    {
                        codigoUsuarioEcar = e.CodigoEmpleadoEcar,
                        nombreEmpleado = e.NombreEmpleado,
                        usuario = e.FirmaBpm
                    })
                    .FirstOrDefaultAsync();

                if (empleado == null)
                {
                    Console.WriteLine("No se encontró empleado con ese código");
                    return Json(new { success = false, message = "No se encontró ningún empleado con ese código." });
                }

                Console.WriteLine($"Empleado encontrado: {empleado.nombreEmpleado}");
                return Json(new { success = true, data = empleado });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al buscar empleado: {ex.Message}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // GET: Empleado/Index
        public async Task<IActionResult> Index(string searchString, string estadoFilter, string sortOrder, string cargoFilter, string areaFilter)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["EstadoFilter"] = estadoFilter;
            ViewData["CargoFilter"] = cargoFilter;
            ViewData["AreaFilter"] = areaFilter;

            // Parámetros de ordenamiento - AGREGADO EstadoSortParam
            ViewData["NombreSortParam"] = string.IsNullOrEmpty(sortOrder) ? "nombre_desc" : "";
            ViewData["CodigoSortParam"] = sortOrder == "Codigo" ? "codigo_desc" : "Codigo";
            ViewData["EstadoSortParam"] = sortOrder == "Estado" ? "estado_desc" : "Estado"; // ← NUEVO
            ViewData["CargoSortParam"] = sortOrder == "Cargo" ? "cargo_desc" : "Cargo";
            ViewData["AreaSortParam"] = sortOrder == "Area" ? "area_desc" : "Area";

            var empleados = from e in _context.Empleados
                            select e;

            if (!string.IsNullOrEmpty(searchString))
            {
                empleados = empleados.Where(e =>
                    e.CodigoEmpleadoEcar.Contains(searchString) ||
                    e.NombreEmpleado.Contains(searchString));
            }

            // Filtro de estado - SIN modificar el comportamiento existente
            if (!string.IsNullOrEmpty(estadoFilter))
            {
                if (estadoFilter.Equals("Activo", StringComparison.OrdinalIgnoreCase))
                {
                    empleados = empleados.Where(e => e.Estado == "Activo");
                }
                else if (estadoFilter.Equals("Inactivo", StringComparison.OrdinalIgnoreCase))
                {
                    empleados = empleados.Where(e => e.Estado == "Inactivo");
                }
            }

            if (!string.IsNullOrEmpty(cargoFilter))
            {
                empleados = empleados.Where(e => e.Cargo == cargoFilter);
            }

            if (!string.IsNullOrEmpty(areaFilter))
            {
                empleados = empleados.Where(e => e.Area == areaFilter);
            }

            // Switch de ordenamiento - AGREGADOS casos para Estado
            switch (sortOrder)
            {
                case "nombre_desc":
                    empleados = empleados.OrderByDescending(e => e.NombreEmpleado);
                    break;
                case "Codigo":
                    empleados = empleados.OrderBy(e => e.CodigoEmpleadoEcar);
                    break;
                case "codigo_desc":
                    empleados = empleados.OrderByDescending(e => e.CodigoEmpleadoEcar);
                    break;
                case "Estado": // ← NUEVO
                    empleados = empleados.OrderBy(e => e.Estado);
                    break;
                case "estado_desc": // ← NUEVO
                    empleados = empleados.OrderByDescending(e => e.Estado);
                    break;
                case "Cargo":
                    empleados = empleados.OrderBy(e => e.Cargo);
                    break;
                case "cargo_desc":
                    empleados = empleados.OrderByDescending(e => e.Cargo);
                    break;
                case "Area":
                    empleados = empleados.OrderBy(e => e.Area);
                    break;
                case "area_desc":
                    empleados = empleados.OrderByDescending(e => e.Area);
                    break;
                default:
                    empleados = empleados.OrderBy(e => e.NombreEmpleado);
                    break;
            }

            var empleadosList = await empleados.ToListAsync();
            ViewData["TotalRegistros"] = empleadosList.Count;

            ViewBag.Cargos = await _context.Empleados.Select(e => e.Cargo).Distinct().Where(c => !string.IsNullOrEmpty(c)).OrderBy(c => c).ToListAsync();
            ViewBag.Areas = await _context.Empleados.Select(e => e.Area).Distinct().Where(a => !string.IsNullOrEmpty(a)).OrderBy(a => a).ToListAsync();

            return View(empleadosList);
        }

        // GET: Empleado/BuscarEmpleados
        public async Task<IActionResult> BuscarEmpleados(string term)
        {
            var empleadosFiltrados = await _context.Empleados
                .Where(e => e.CodigoEmpleadoEcar.Contains(term) || e.NombreEmpleado.Contains(term))
                .ToListAsync();

            return Json(empleadosFiltrados);
        }

        // GET: Empleados/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var empleado = await _context.Empleados
                .FirstOrDefaultAsync(m => m.Id == id);
            if (empleado == null)
            {
                return NotFound();
            }

            return View(empleado);
        }

        // GET: Empleados/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Empleados/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CodigoEmpleadoEcar,Fecha,NombreEmpleado,Cargo,Area,SubArea,Nota,Estado,UsuarioRegistro,FirmaBpm")] Empleado empleado)
        {
            if (await _context.Empleados.AnyAsync(e => e.CodigoEmpleadoEcar == empleado.CodigoEmpleadoEcar))
            {
                ModelState.AddModelError("CodigoEmpleadoEcar", "Este código de empleado ya existe. Por favor, ingrese uno diferente.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(empleado);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Empleado '{empleado.NombreEmpleado}' creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Error al crear el empleado. Revise los datos.";
            return View(empleado);
        }

        // GET: Empleados/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null)
            {
                return NotFound();
            }
            return View(empleado);
        }

        // POST: Empleados/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CodigoEmpleadoEcar,Fecha,NombreEmpleado,Cargo,Area,SubArea,Nota,Estado,UsuarioRegistro,FirmaBpm")] Empleado empleado)
        {
            if (id != empleado.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(empleado);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Empleado '{empleado.NombreEmpleado}' actualizado exitosamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmpleadoExists(id))
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
            TempData["ErrorMessage"] = "Error al actualizar el empleado. Revise los datos.";
            return View(empleado);
        }

        [HttpPost, ActionName("Activar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivarConfirmado(int id)
        {
            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null)
            {
                TempData["ErrorMessage"] = "El empleado que intentó activar no fue encontrado.";
                return NotFound();
            }

            empleado.Estado = "Activo";

            try
            {
                _context.Update(empleado);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Empleado '{empleado.NombreEmpleado}' activado exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al activar el empleado: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Empleados/Desactivar/5
        [HttpPost, ActionName("Desactivar")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DesactivarConfirmado(int id)
        {
            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null)
            {
                TempData["ErrorMessage"] = "El empleado que intentó desactivar no fue encontrado.";
                return NotFound();
            }

            // Verificar si tiene credenciales activas (solo para mostrar advertencia)
            var credencialesActivas = await _context.Credenciales
                                            .Where(c => c.CodigoUsuarioEcar == empleado.CodigoEmpleadoEcar && c.Estado == "Activo")
                                            .AnyAsync();

            empleado.Estado = "Inactivo";

            try
            {
                _context.Update(empleado);
                await _context.SaveChangesAsync();

                // Mensaje de éxito con advertencia si tiene credenciales activas
                if (credencialesActivas)
                {
                    TempData["SuccessMessage"] = $"Empleado '{empleado.NombreEmpleado}' inactivado exitosamente. " +
                                               $"<strong>ADVERTENCIA:</strong> Este empleado tiene credenciales activas asociadas. " +
                                               $"Por favor, revise y desactive las credenciales manualmente.";
                }
                else
                {
                    TempData["SuccessMessage"] = $"Empleado '{empleado.NombreEmpleado}' inactivado exitosamente.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al inactivar el empleado: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Dentro de la clase EmpleadosController, en la parte inferior.
        private bool EmpleadoExists(int id)
        {
            return _context.Empleados.Any(e => e.Id == id);
        }
    }
}