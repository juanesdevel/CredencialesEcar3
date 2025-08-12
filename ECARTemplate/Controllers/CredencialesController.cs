using ECARTemplate.Data;
using ECARTemplate.Models;
using ECARTemplate.Services; // Importa tu nuevo servicio AES
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography; // Agrega esta línea para CryptographicException

namespace ECARTemplate.Controllers
{
    [Authorize]
    public class CredencialesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CredencialesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Credenciales
        public async Task<IActionResult> Index(
            string codigoEquipoFiltro,
            DateTime? fechaYHoraDesdeFiltro, // ¡CORREGIDO! El nombre ahora coincide con la vista
            DateTime? fechaYHoraHastaFiltro, // ¡CORREGIDO! El nombre ahora coincide con la vista
            string codigoUsuarioEcarFiltro,
            string nombreUsuarioFiltro,
            string perfilFiltro,
            string usuarioFiltro,
            string estadoFiltro,
            string usuarioRegistroFiltro)
        {
            if (!fechaYHoraDesdeFiltro.HasValue && !fechaYHoraHastaFiltro.HasValue)
            {
                fechaYHoraDesdeFiltro = DateTime.Today;
                fechaYHoraHastaFiltro = DateTime.Today.AddDays(1).AddSeconds(-1);
            }

            var credenciales = _context.Credenciales.AsQueryable();

            if (!string.IsNullOrEmpty(codigoEquipoFiltro))
            {
                credenciales = credenciales.Where(c => c.CodigoEquipo.Contains(codigoEquipoFiltro));
            }

            if (fechaYHoraDesdeFiltro.HasValue)
            {
                credenciales = credenciales.Where(c => c.FechaYHora >= fechaYHoraDesdeFiltro.Value);
            }
            if (fechaYHoraHastaFiltro.HasValue)
            {
                credenciales = credenciales.Where(c => c.FechaYHora <= fechaYHoraHastaFiltro.Value.Date.AddDays(1).AddSeconds(-1));
            }

            if (!string.IsNullOrEmpty(codigoUsuarioEcarFiltro))
            {
                credenciales = credenciales.Where(c => c.CodigoUsuarioEcar.Contains(codigoUsuarioEcarFiltro));
            }

            if (!string.IsNullOrEmpty(nombreUsuarioFiltro))
            {
                credenciales = credenciales.Where(c => c.NombreUsuario.Contains(nombreUsuarioFiltro));
            }

            if (!string.IsNullOrEmpty(perfilFiltro))
            {
                credenciales = credenciales.Where(c => c.Perfil.Contains(perfilFiltro));
            }

            if (!string.IsNullOrEmpty(usuarioFiltro))
            {
                credenciales = credenciales.Where(c => c.Usuario.Contains(usuarioFiltro));
            }

            if (!string.IsNullOrEmpty(estadoFiltro))
            {
                credenciales = credenciales.Where(c => c.Estado.Contains(estadoFiltro));
            }

            if (!string.IsNullOrEmpty(usuarioRegistroFiltro))
            {
                credenciales = credenciales.Where(c => c.UsuarioRegistro.Contains(usuarioRegistroFiltro));
            }

            var credencialesList = await credenciales.ToListAsync();

            foreach (var credencial in credencialesList)
            {
                if (!string.IsNullOrEmpty(credencial.Contrasena))
                {
                    try
                    {
                        credencial.Contrasena = AesEncryptor.Decrypt(credencial.Contrasena);
                    }
                    catch (FormatException)
                    {
                        credencial.Contrasena = "[Contraseña Inválida]";
                    }
                    catch (CryptographicException)
                    {
                        credencial.Contrasena = "[Error Criptográfico]";
                    }
                    catch (Exception)
                    {
                        credencial.Contrasena = "[Error Desconocido]";
                    }
                }
            }

            ViewData["TotalRegistros"] = credencialesList.Count;
            ViewBag.CodigoEquipoFiltro = codigoEquipoFiltro;
            ViewBag.FechaYHoraDesdeFiltro = fechaYHoraDesdeFiltro;
            ViewBag.FechaYHoraHastaFiltro = fechaYHoraHastaFiltro;
            ViewBag.CodigoUsuarioEcarFiltro = codigoUsuarioEcarFiltro;
            ViewBag.NombreUsuarioFiltro = nombreUsuarioFiltro;
            ViewBag.PerfilFiltro = perfilFiltro;
            ViewBag.UsuarioFiltro = usuarioFiltro;
            ViewBag.EstadoFiltro = estadoFiltro;
            ViewBag.UsuarioRegistroFiltro = usuarioRegistroFiltro;

            return View(credencialesList);
        }
        // GET: Credenciales/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var credencial = await _context.Credenciales
                .FirstOrDefaultAsync(m => m.Id == id);
            if (credencial == null)
            {
                return NotFound();
            }

            // Desencriptamos la contraseña para mostrarla en el detalle
            if (!string.IsNullOrEmpty(credencial.Contrasena))
            {
                try
                {
                    credencial.Contrasena = AesEncryptor.Decrypt(credencial.Contrasena);
                }
                catch (FormatException)
                {
                    credencial.Contrasena = "[Contraseña Inválida/No Encriptada]";
                    TempData["WarningMessage"] = "La contraseña de esta credencial no pudo ser desencriptada. Puede estar en un formato incorrecto o no encriptada.";
                }
                catch (CryptographicException)
                {
                    credencial.Contrasena = "[Error de Desencriptación]";
                    TempData["ErrorMessage"] = "Hubo un error criptográfico al intentar desencriptar la contraseña. Revise la clave o los datos.";
                }
                catch (Exception ex)
                {
                    credencial.Contrasena = "[Error de Desencriptación]";
                    TempData["ErrorMessage"] = $"Error al desencriptar: {ex.Message}";
                }
            }
            return View(credencial);
        }

        // GET: Credenciales/Create
        public async Task<IActionResult> Create(int? empleadoId)
        {
            var credencial = new Credencial
            {
                FechaYHora = DateTime.Now,
                Estado = "Activo",
                UsuarioRegistro = User?.Identity?.Name
            };

            if (empleadoId.HasValue)
            {
                var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.Id == empleadoId.Value);
                if (empleado != null)
                {
                    if (empleado.Estado == "Activo")
                    {
                        credencial.CodigoUsuarioEcar = empleado.CodigoEmpleadoEcar;
                        credencial.NombreUsuario = empleado.NombreEmpleado;
                        credencial.Usuario = empleado.FirmaBpm;
                    }
                    else
                    {
                        TempData["ErrorMessage"] = $"El empleado '{empleado.NombreEmpleado}' se encuentra inactivo. No se puede crear una credencial para él.";
                    }
                }
            }
            return View(credencial);
        }

        // POST: Credenciales/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Credencial credencial)
        {
            var equipoAsociado = await _context.Equipos.FirstOrDefaultAsync(e => e.CodigoEquipo == credencial.CodigoEquipo);
            if (equipoAsociado == null || equipoAsociado.Estado != "Activo")
            {
                ModelState.AddModelError(nameof(credencial.CodigoEquipo), $"El equipo '{credencial.CodigoEquipo}' no existe o no está activo. No se puede crear la credencial.");
                return View(credencial);
            }

            var empleadoAsociado = await _context.Empleados.FirstOrDefaultAsync(e => e.CodigoEmpleadoEcar == credencial.CodigoUsuarioEcar);
            if (empleadoAsociado == null || empleadoAsociado.Estado != "Activo")
            {
                ModelState.AddModelError(nameof(credencial.CodigoUsuarioEcar), $"El empleado '{credencial.CodigoUsuarioEcar}' no existe o no está activo. No se puede crear la credencial.");
                return View(credencial);
            }

            bool existeCredencial = await _context.Credenciales
                .AnyAsync(c => c.CodigoEquipo == credencial.CodigoEquipo && c.CodigoUsuarioEcar == credencial.CodigoUsuarioEcar);

            if (existeCredencial)
            {
                ModelState.AddModelError(nameof(credencial.CodigoUsuarioEcar),
                    $"Ya existe un registro para el usuario {credencial.CodigoUsuarioEcar} en el equipo {credencial.CodigoEquipo}.");
                return View(credencial);
            }

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(credencial.Contrasena))
                {
                    credencial.Contrasena = AesEncryptor.Encrypt(credencial.Contrasena);
                }
                else
                {
                    ModelState.AddModelError(nameof(credencial.Contrasena), "La contraseña es requerida.");
                    return View(credencial);
                }

                _context.Add(credencial);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"La credencial para el equipo '{credencial.CodigoEquipo}' y usuario '{credencial.NombreUsuario}' ha sido creada exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            return View(credencial);
        }

        // GET: Credenciales/Edit/5
        [HttpGet] // <-- ¡SE AÑADIÓ ESTA LÍNEA!
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var credencial = await _context.Credenciales.FindAsync(id);
            if (credencial == null)
            {
                return NotFound();
            }
            credencial.Contrasena = string.Empty;
            return View(credencial);
        }

        // POST: Credenciales/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CodigoEquipo,FechaYHora,CodigoUsuarioEcar,NombreUsuario,Perfil,Usuario,Contrasena,Estado,UsuarioRegistro")] Credencial credencial)
        {
            if (id != credencial.Id)
            {
                return NotFound();
            }

            var equipoAsociado = await _context.Equipos.FirstOrDefaultAsync(e => e.CodigoEquipo == credencial.CodigoEquipo);
            if (equipoAsociado == null || equipoAsociado.Estado != "Activo")
            {
                ModelState.AddModelError(nameof(credencial.CodigoEquipo), $"El equipo '{credencial.CodigoEquipo}' no existe o no está activo. No se puede actualizar la credencial.");
                return View(credencial);
            }

            var empleadoAsociado = await _context.Empleados.FirstOrDefaultAsync(e => e.CodigoEmpleadoEcar == credencial.CodigoUsuarioEcar);
            if (empleadoAsociado == null || empleadoAsociado.Estado != "Activo")
            {
                ModelState.AddModelError(nameof(credencial.CodigoUsuarioEcar), $"El empleado '{credencial.CodigoUsuarioEcar}' no existe o no está activo. No se puede actualizar la credencial.");
                return View(credencial);
            }

            bool existeCredencialDuplicada = await _context.Credenciales
                .AnyAsync(c => c.CodigoEquipo == credencial.CodigoEquipo && c.CodigoUsuarioEcar == credencial.CodigoUsuarioEcar && c.Id != credencial.Id);

            if (existeCredencialDuplicada)
            {
                ModelState.AddModelError(nameof(credencial.CodigoUsuarioEcar),
                    $"Ya existe un registro para el usuario {credencial.CodigoUsuarioEcar} en el equipo {credencial.CodigoEquipo}.");
                return View(credencial);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var credencialToUpdate = await _context.Credenciales.FindAsync(id);
                    if (credencialToUpdate == null)
                    {
                        return NotFound();
                    }

                    if (!string.IsNullOrEmpty(credencial.Contrasena))
                    {
                        credencialToUpdate.Contrasena = AesEncryptor.Encrypt(credencial.Contrasena);
                    }

                    credencialToUpdate.CodigoEquipo = credencial.CodigoEquipo;
                    credencialToUpdate.FechaYHora = credencial.FechaYHora;
                    credencialToUpdate.CodigoUsuarioEcar = credencial.CodigoUsuarioEcar;
                    credencialToUpdate.NombreUsuario = credencial.NombreUsuario;
                    credencialToUpdate.Perfil = credencial.Perfil;
                    credencialToUpdate.Usuario = credencial.Usuario;
                    credencialToUpdate.Estado = credencial.Estado;
                    credencialToUpdate.UsuarioRegistro = credencial.UsuarioRegistro;

                    _context.Update(credencialToUpdate);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"La credencial para el equipo '{credencial.CodigoEquipo}' y usuario '{credencial.NombreUsuario}' ha sido actualizada exitosamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CredencialExists(credencial.Id))
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
            return View(credencial);
        }

        // GET: Credenciales/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var credencial = await _context.Credenciales
                .FirstOrDefaultAsync(m => m.Id == id);
            if (credencial == null)
            {
                return NotFound();
            }
            return View(credencial);
        }

        // POST: Credenciales/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var credencial = await _context.Credenciales.FindAsync(id);
            if (credencial == null)
            {
                TempData["ErrorMessage"] = "La credencial que intentó eliminar no fue encontrada.";
                return RedirectToAction(nameof(Index));
            }

            _context.Credenciales.Remove(credencial);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"La credencial para el equipo '{credencial.CodigoEquipo}' y usuario '{credencial.NombreUsuario}' ha sido eliminada exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Credenciales/Activar/5
        public async Task<IActionResult> Activar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var credencial = await _context.Credenciales.FindAsync(id);
            if (credencial == null)
            {
                return NotFound();
            }

            if (credencial.Estado == "Inactivo")
            {
                var equipoAsociado = await _context.Equipos
                    .FirstOrDefaultAsync(e => e.CodigoEquipo == credencial.CodigoEquipo);

                if (equipoAsociado == null || equipoAsociado.Estado != "Activo")
                {
                    TempData["ErrorMessage"] = $"No es posible activar la credencial porque el equipo '{credencial.CodigoEquipo}' asociado se encuentra {(equipoAsociado == null ? "no encontrado o" : "")} inactivo.";
                    return RedirectToAction(nameof(Index));
                }

                var empleadoAsociado = await _context.Empleados
                    .FirstOrDefaultAsync(e => e.CodigoEmpleadoEcar == credencial.CodigoUsuarioEcar);

                if (empleadoAsociado == null || empleadoAsociado.Estado != "Activo")
                {
                    TempData["ErrorMessage"] = $"No es posible activar la credencial porque el empleado '{credencial.CodigoUsuarioEcar}' asociado se encuentra {(empleadoAsociado == null ? "no encontrado o" : "")} inactivo.";
                    return RedirectToAction(nameof(Index));
                }

                credencial.Estado = "Activo";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"La credencial para el empleado '{credencial.NombreUsuario}' en el equipo '{credencial.CodigoEquipo}' ha sido activada correctamente.";
            }
            else if (credencial.Estado == "Activo")
            {
                TempData["InfoMessage"] = $"La credencial para el empleado '{credencial.NombreUsuario}' en el equipo '{credencial.CodigoEquipo}' ya se encuentra activa.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Credenciales/Desactivar/5
        public async Task<IActionResult> Desactivar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var credencial = await _context.Credenciales.FindAsync(id);
            if (credencial == null)
            {
                return NotFound();
            }

            credencial.Estado = "Inactivo";
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"La credencial para el empleado '{credencial.NombreUsuario}' en el equipo '{credencial.CodigoEquipo}' ha sido inactivada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private bool CredencialExists(int id)
        {
            return _context.Credenciales.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerDatosEmpleadoPorCodigo(string codigoEmpleado)
        {
            if (string.IsNullOrEmpty(codigoEmpleado))
            {
                return Json(new { success = false, message = "El Código de Empleado es requerido." });
            }

            var empleado = await _context.Empleados
                .Where(e => e.CodigoEmpleadoEcar == codigoEmpleado)
                .Select(e => new
                {
                    e.CodigoEmpleadoEcar,
                    e.NombreEmpleado,
                    Usuario = e.FirmaBpm
                })
                .FirstOrDefaultAsync();

            if (empleado == null)
            {
                return Json(new { success = false, message = "No se encontró ningún empleado con ese código." });
            }

            return Json(new { success = true, data = empleado });
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerDatosEquipo(string referencia)
        {
            if (string.IsNullOrEmpty(referencia))
            {
                return Json(new { success = false, message = "La referencia del equipo es requerida." });
            }

            var equipo = await _context.Equipos
                .Where(e => e.CodigoEquipo == referencia || e.NombreEquipo.Contains(referencia))
                .Select(e => new
                {
                    e.CodigoEquipo,
                    e.NombreEquipo,
                    e.Nota
                })
                .FirstOrDefaultAsync();

            if (equipo == null)
            {
                return Json(new { success = false, message = "No se encontró ningún equipo con esa referencia." });
            }

            return Json(new { success = true, data = equipo });
        }

        // GET: Credenciales/Clonar/5
        public async Task<IActionResult> Clonar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var credencial = await _context.Credenciales.FindAsync(id);
            if (credencial == null)
            {
                return NotFound();
            }

            var credencialClon = new Credencial
            {
                CodigoEquipo = "",
                FechaYHora = DateTime.Now,
                CodigoUsuarioEcar = credencial.CodigoUsuarioEcar,
                NombreUsuario = credencial.NombreUsuario,
                Perfil = credencial.Perfil,
                Usuario = credencial.Usuario,
                Contrasena = string.Empty,
                Estado = credencial.Estado,
                UsuarioRegistro = User?.Identity?.Name
            };

            ViewBag.CredencialOriginalId = id;

            return View("Clonar", credencialClon);
        }

        // POST: Credenciales/GuardarClon
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarClon(Credencial credencial)
        {
            var equipoAsociado = await _context.Equipos.FirstOrDefaultAsync(e => e.CodigoEquipo == credencial.CodigoEquipo);
            if (equipoAsociado == null || equipoAsociado.Estado != "Activo")
            {
                ModelState.AddModelError(nameof(credencial.CodigoEquipo), $"El equipo '{credencial.CodigoEquipo}' no existe o no está activo. No se puede clonar la credencial.");
                return View("Clonar", credencial);
            }

            var empleadoAsociado = await _context.Empleados.FirstOrDefaultAsync(e => e.CodigoEmpleadoEcar == credencial.CodigoUsuarioEcar);
            if (empleadoAsociado == null || empleadoAsociado.Estado != "Activo")
            {
                ModelState.AddModelError(nameof(credencial.CodigoUsuarioEcar), $"El empleado '{credencial.CodigoUsuarioEcar}' no existe o no está activo. No se puede clonar la credencial.");
                return View("Clonar", credencial);
            }

            bool existeCredencial = await _context.Credenciales
                .AnyAsync(c => c.CodigoEquipo == credencial.CodigoEquipo && c.CodigoUsuarioEcar == credencial.CodigoUsuarioEcar);

            if (existeCredencial)
            {
                ModelState.AddModelError(nameof(credencial.CodigoUsuarioEcar),
                    $"Ya existe un registro para el usuario {credencial.CodigoUsuarioEcar} en el equipo {credencial.CodigoEquipo}.");
                return View("Clonar", credencial);
            }

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(credencial.Contrasena))
                {
                    credencial.Contrasena = AesEncryptor.Encrypt(credencial.Contrasena);
                }
                else
                {
                    ModelState.AddModelError(nameof(credencial.Contrasena), "La contraseña es requerida para la credencial clonada.");
                    return View("Clonar", credencial);
                }

                credencial.Id = 0;
                credencial.FechaYHora = DateTime.Now;

                _context.Add(credencial);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "La credencial se ha clonado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View("Clonar", credencial);
        }
    }
}