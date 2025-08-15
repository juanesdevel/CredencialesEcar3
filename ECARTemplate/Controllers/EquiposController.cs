using Microsoft.AspNetCore.Mvc;
using ECARTemplate.Data;
using ECARTemplate.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace ECARTemplate.Controllers
{
    [Authorize]
    public class EquiposController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EquiposController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

// GET: Equipos/Index
public async Task<IActionResult> Index(string searchString, string sedeFilter, string areaFilter, string estadoFilter, string sortOrder)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["SedeFilter"] = sedeFilter;
            ViewData["AreaFilter"] = areaFilter;
            ViewData["EstadoFilter"] = estadoFilter;

            // Parámetros de ordenamiento
            ViewData["NombreSortParam"] = sortOrder == "Nombre" ? "nombre_desc" : "Nombre";
            ViewData["CodigoSortParam"] = sortOrder == "Codigo" ? "codigo_desc" : "Codigo";
            ViewData["EstadoSortParam"] = sortOrder == "Estado" ? "estado_desc" : "Estado";
            ViewData["SedeSortParam"] = sortOrder == "Sede" ? "sede_desc" : "Sede";
            ViewData["AreaSortParam"] = sortOrder == "Area" ? "area_desc" : "Area";

            var equipos = _context.Equipos.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                equipos = equipos.Where(e =>
                    e.CodigoEquipo.Contains(searchString) ||
                    e.NombreEquipo.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(sedeFilter))
            {
                equipos = equipos.Where(e => e.Sede == sedeFilter);
            }

            if (!string.IsNullOrEmpty(areaFilter))
            {
                equipos = equipos.Where(e => e.Area == areaFilter);
            }

            // FILTRO DE ESTADO CORREGIDO - Solo filtra cuando se especifica
            if (!string.IsNullOrEmpty(estadoFilter))
            {
                if (estadoFilter.Equals("Activo", StringComparison.OrdinalIgnoreCase))
                {
                    equipos = equipos.Where(e => e.Estado == "Activo");
                }
                else if (estadoFilter.Equals("Inactivo", StringComparison.OrdinalIgnoreCase))
                {
                    equipos = equipos.Where(e => e.Estado == "Inactivo");
                }
                // Si es "Todos" o cualquier otro valor, no filtramos por estado
            }
            // ELIMINADO: El else que forzaba mostrar solo activos

            // Lógica de ordenamiento
            switch (sortOrder)
            {
                case "nombre_desc":
                    equipos = equipos.OrderByDescending(e => e.NombreEquipo);
                    break;
                case "Codigo":
                    equipos = equipos.OrderBy(e => e.CodigoEquipo);
                    break;
                case "codigo_desc":
                    equipos = equipos.OrderByDescending(e => e.CodigoEquipo);
                    break;
                case "Estado":
                    equipos = equipos.OrderBy(e => e.Estado);
                    break;
                case "estado_desc":
                    equipos = equipos.OrderByDescending(e => e.Estado);
                    break;
                case "Sede":
                    equipos = equipos.OrderBy(e => e.Sede);
                    break;
                case "sede_desc":
                    equipos = equipos.OrderByDescending(e => e.Sede);
                    break;
                case "Area":
                    equipos = equipos.OrderBy(e => e.Area);
                    break;
                case "area_desc":
                    equipos = equipos.OrderByDescending(e => e.Area);
                    break;
                default:
                    equipos = equipos.OrderBy(e => e.NombreEquipo);
                    break;
            }

            var equiposList = await equipos.ToListAsync();

            ViewBag.Sedes = await _context.Equipos
                .Select(e => e.Sede)
                .Distinct()
                .Where(s => !string.IsNullOrEmpty(s))
                .OrderBy(s => s)
                .ToListAsync();

            ViewBag.Areas = await _context.Equipos
                .Select(e => e.Area)
                .Distinct()
                .Where(a => !string.IsNullOrEmpty(a))
                .OrderBy(a => a)
                .ToListAsync();

            ViewData["TotalRegistros"] = equiposList.Count;

            return View(equiposList);
        }

        // GET: Equipos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var equipo = await _context.Equipos.FirstOrDefaultAsync(m => m.Id == id);
            if (equipo == null) return NotFound();
            return View(equipo);
        }

        // GET: Equipos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Equipos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CodigoEquipo,Fecha,NombreEquipo,Sede,Area,SubArea,Nota,Estado,RutaImagen,HojaDeVida")] Equipo equipo, IFormFile imageFile)
        {
            if (await _context.Equipos.AnyAsync(e => e.CodigoEquipo == equipo.CodigoEquipo))
            {
                ModelState.AddModelError("CodigoEquipo", "Ya existe un equipo con este código.");
            }

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "equipos");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }
                    equipo.RutaImagen = "/images/equipos/" + uniqueFileName;
                }

                equipo.UsuarioRegistro = User.Identity.Name;
                equipo.Fecha = DateTime.Now;

                _context.Add(equipo);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Equipo '{equipo.NombreEquipo}' creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Error al crear el equipo. Revise los datos.";
            return View(equipo);
        }

        // GET: Equipos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var equipo = await _context.Equipos.FindAsync(id);
            if (equipo == null) return NotFound();
            return View(equipo);
        }

        // POST: Equipos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CodigoEquipo,Fecha,NombreEquipo,Sede,Area,SubArea,Nota,Estado,RutaImagen,HojaDeVida")] Equipo equipo, IFormFile imageFile)
        {
            if (id != equipo.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (await _context.Equipos.AnyAsync(e => e.CodigoEquipo == equipo.CodigoEquipo && e.Id != equipo.Id))
                    {
                        ModelState.AddModelError("CodigoEquipo", "Ya existe otro equipo con este código.");
                        return View(equipo);
                    }

                    var equipoToUpdate = await _context.Equipos.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
                    if (equipoToUpdate == null) return NotFound();

                    if (imageFile != null && imageFile.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(equipoToUpdate.RutaImagen))
                        {
                            string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, equipoToUpdate.RutaImagen.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                        }
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "equipos");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }
                        equipo.RutaImagen = "/images/equipos/" + uniqueFileName;
                    }
                    else
                    {
                        equipo.RutaImagen = equipoToUpdate.RutaImagen;
                    }

                    equipo.Fecha = DateTime.Now;
                    equipo.UsuarioRegistro = User.Identity.Name;

                    _context.Update(equipo);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Equipo '{equipo.NombreEquipo}' actualizado exitosamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EquipoExists(equipo.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Error al actualizar el equipo. Revise los datos.";
            return View(equipo);
        }

        // GET: Equipos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var equipo = await _context.Equipos.FirstOrDefaultAsync(m => m.Id == id);
            if (equipo == null) return NotFound();
            return View(equipo);
        }

        // POST: Equipos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var equipo = await _context.Equipos.FindAsync(id);
            if (equipo == null)
            {
                TempData["ErrorMessage"] = "El equipo que intentó eliminar no fue encontrado.";
                return NotFound();
            }

            if (!string.IsNullOrEmpty(equipo.RutaImagen))
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, equipo.RutaImagen.TrimStart('/'));
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            }

            _context.Equipos.Remove(equipo);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Equipo '{equipo.NombreEquipo}' eliminado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Equipos/Activar/5
        [HttpPost, ActionName("Activar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivarConfirmed(int id)
        {
            var equipo = await _context.Equipos.FindAsync(id);
            if (equipo == null)
            {
                TempData["ErrorMessage"] = "El equipo que intentó activar no fue encontrado.";
                return NotFound();
            }

            equipo.Estado = "Activo";

            try
            {
                _context.Update(equipo);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Equipo '{equipo.NombreEquipo}' activado exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al activar el equipo: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Equipos/Inactivar/5
        [HttpPost, ActionName("Inactivar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InactivarConfirmed(int id)
        {
            var equipo = await _context.Equipos.FindAsync(id);
            if (equipo == null)
            {
                TempData["ErrorMessage"] = "El equipo que intentó inactivar no fue encontrado.";
                return NotFound();
            }

            // Validar si existen credenciales activas para este equipo
            var credencialesActivas = await _context.Credenciales
                                                    .Where(c => c.CodigoEquipo == equipo.CodigoEquipo && c.Estado == "Activo")
                                                    .AnyAsync();

            if (credencialesActivas)
            {
                TempData["ErrorMessage"] = "No se puede inactivar el equipo. Aún hay credenciales activas asociadas a este equipo. Por favor, revise y desactive las credenciales primero.";
                return RedirectToAction(nameof(Index));
            }

            // Si no hay credenciales activas, proceder a inactivar el equipo
            equipo.Estado = "Inactivo";
            equipo.Fecha = DateTime.Now; // Actualizar fecha de modificación
            equipo.UsuarioRegistro = User.Identity.Name; // Actualizar usuario que hizo la modificación

            try
            {
                _context.Update(equipo);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Equipo '{equipo.NombreEquipo}' inactivado exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al inactivar el equipo: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
        private bool EquipoExists(int id)
        {
            return _context.Equipos.Any(e => e.Id == id);
        }
        // Método para debugging - puedes llamarlo temporalmente para verificar el estado
        [HttpGet]
        public async Task<IActionResult> DebugEquipo(int id)
        {
            var equipo = await _context.Equipos.FindAsync(id);
            if (equipo == null) return NotFound();

            var credencialesActivas = await _context.Credenciales
                                                   .Where(c => c.CodigoEquipo == equipo.CodigoEquipo && c.Estado == "Activo")
                                                   .ToListAsync();

            return Json(new
            {
                equipoId = equipo.Id,
                codigoEquipo = equipo.CodigoEquipo,
                nombreEquipo = equipo.NombreEquipo,
                estadoEquipo = equipo.Estado,
                credencialesActivas = credencialesActivas.Select(c => new
                {
                    id = c.Id,
                    codigo = c.CodigoEquipo,
                    estado = c.Estado
                }).ToList()
            });
        }
    }
}