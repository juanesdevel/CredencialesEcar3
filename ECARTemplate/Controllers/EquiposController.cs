using Microsoft.AspNetCore.Mvc;
using ECARTemplate.Data;
using ECARTemplate.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.Data.SqlClient;

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

                // Asignar el nombre del usuario de registro del Directorio Activo
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

                    // Asignar el nombre del usuario de registro del Directorio Activo
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

                var connection = _context.Database.GetDbConnection();
                var spOutput = new SpResult { Success = 0, Message = "Error desconocido al ejecutar SP." };

                try
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "dbo.SP_Equipos_ActualizarEstado_Cascade";
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@IdEquipo", equipo.Id));
                        command.Parameters.Add(new SqlParameter("@NuevoEstado", equipo.Estado));

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                spOutput.Success = reader.GetInt32(reader.GetOrdinal("Success"));
                                spOutput.Message = reader.GetString(reader.GetOrdinal("Message"));
                            }
                        }
                    }
                }
                finally
                {
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        await connection.CloseAsync();
                    }
                }

                if (spOutput.Success == 0)
                {
                    TempData["ErrorMessage"] = $"Equipo activado, pero hubo un error en la cascada de credenciales: {spOutput.Message}";
                }
                else
                {
                    TempData["SuccessMessage"] = $"Equipo '{equipo.NombreEquipo}' activado exitosamente.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al activar equipo o sus credenciales: {ex.Message}";
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

                var connection = _context.Database.GetDbConnection();
                var spOutput = new SpResult { Success = 0, Message = "Error desconocido al ejecutar SP." };

                try
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "dbo.SP_Equipos_ActualizarEstado_Cascade";
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@IdEquipo", equipo.Id));
                        command.Parameters.Add(new SqlParameter("@NuevoEstado", equipo.Estado));

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                spOutput.Success = reader.GetInt32(reader.GetOrdinal("Success"));
                                spOutput.Message = reader.GetString(reader.GetOrdinal("Message"));
                            }
                        }
                    }
                }
                finally
                {
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        await connection.CloseAsync();
                    }
                }

                if (spOutput.Success == 0)
                {
                    TempData["ErrorMessage"] = $"Equipo inactivado, pero hubo un error en la cascada de credenciales: {spOutput.Message}";
                }
                else
                {
                    TempData["SuccessMessage"] = $"Equipo '{equipo.NombreEquipo}' inactivado exitosamente.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al inactivar equipo o sus credenciales: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Método Buscar usado para AJAX
        [HttpGet]
        public async Task<IActionResult> Buscar(string term)
        {
            var equiposFiltrados = await _context.Equipos
                .Where(e => e.CodigoEquipo.Contains(term) || e.NombreEquipo.Contains(term))
                .ToListAsync();
            return Json(equiposFiltrados);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerCodigoPorReferencia(string referencia)
        {
            if (string.IsNullOrEmpty(referencia))
                return Json(new { existe = false, mensaje = "Debe proporcionar un código de equipo." });

            var equipo = await _context.Equipos.FirstOrDefaultAsync(e => e.CodigoEquipo == referencia);

            if (equipo != null)
                return Json(new { existe = true, codigoEquipo = equipo.CodigoEquipo, nombreEquipo = equipo.NombreEquipo });
            else
                return Json(new { existe = false, mensaje = "No se encontró ningún equipo con el código especificado." });
        }

        private bool EquipoExists(int id)
        {
            return _context.Equipos.Any(e => e.Id == id);
        }
    }
}