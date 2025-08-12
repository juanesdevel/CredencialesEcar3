namespace ECARTemplate.Controllers
{
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
        public async Task<IActionResult> Index(string searchString, string sedeFilter, string areaFilter, string estadoFilter)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["SedeFilter"] = sedeFilter;
            ViewData["AreaFilter"] = areaFilter;
            ViewData["EstadoFilter"] = estadoFilter;

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
        public async Task<IActionResult> ActivarConfirmado(int id)
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

        // POST: Equipos/Desactivar/5
        [HttpPost, ActionName("Desactivar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesactivarConfirmado(int id)
        {
            var equipo = await _context.Equipos.FindAsync(id);
            if (equipo == null)
            {
                TempData["ErrorMessage"] = "El equipo que intentó desactivar no fue encontrado.";
                return NotFound();
            }

            equipo.Estado = "Inactivo";

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

        [HttpGet]
        public async Task<IActionResult> ObtenerDatosEquipo(string codigoEquipo)
        {
            if (string.IsNullOrEmpty(codigoEquipo))
            {
                return Json(new { success = false, message = "Debe proporcionar un código de equipo." });
            }

            var equipo = await _context.Equipos
                                       .FirstOrDefaultAsync(e => e.CodigoEquipo.ToUpper() == codigoEquipo.ToUpper());

            if (equipo != null)
            {
                return Json(new
                {
                    success = true,
                    data = new
                    {
                        codigoEquipo = equipo.CodigoEquipo,
                        nombreEquipo = equipo.NombreEquipo,
                        nota = equipo.Nota
                    }
                });
            }
            else
            {
                return Json(new { success = false, message = "No se encontró ningún equipo con ese código." });
            }
        }

        private bool EquipoExists(int id)
        {
            return _context.Equipos.Any(e => e.Id == id);
        }
    }
}