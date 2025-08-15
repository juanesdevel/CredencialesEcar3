using ECARTemplate.Data;
using ECARTemplate.Models;
using ECARTemplate.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient; // Asegúrate de tener este 'using'

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

        [HttpGet]
        public async Task<IActionResult> Index(
            string codigoEquipoFiltro,
            DateTime? fechaYHoraDesdeFiltro,
            DateTime? fechaYHoraHastaFiltro,
            string codigoUsuarioEcarFiltro,
            string nombreUsuarioFiltro,
            string perfilFiltro,
            string usuarioFiltro,
            string estadoFiltro,
            string usuarioRegistroFiltro,
            bool retirosPendientes = false,
            string sortOrder = "",
            bool aplicarFiltroHoy = true) // ← NUEVO parámetro
        {
            ViewData["CodigoEquipoFiltro"] = codigoEquipoFiltro;
            ViewData["FechaYHoraDesdeFiltro"] = fechaYHoraDesdeFiltro;
            ViewData["FechaYHoraHastaFiltro"] = fechaYHoraHastaFiltro;
            ViewData["CodigoUsuarioEcarFiltro"] = codigoUsuarioEcarFiltro;
            ViewData["NombreUsuarioFiltro"] = nombreUsuarioFiltro;
            ViewData["PerfilFiltro"] = perfilFiltro;
            ViewData["UsuarioFiltro"] = usuarioFiltro;
            ViewData["EstadoFiltro"] = estadoFiltro;
            ViewData["UsuarioRegistroFiltro"] = usuarioRegistroFiltro;
            ViewData["RetirosPendientes"] = retirosPendientes;

            // Parámetros de ordenamiento
            ViewData["NombreSortParam"] = sortOrder == "Nombre" ? "nombre_desc" : "Nombre";
            ViewData["CodigoEquipoSortParam"] = sortOrder == "CodigoEquipo" ? "codigoequipo_desc" : "CodigoEquipo";
            ViewData["CodigoEmpleadoSortParam"] = sortOrder == "CodigoEmpleado" ? "codigoempleado_desc" : "CodigoEmpleado";
            ViewData["EstadoSortParam"] = sortOrder == "Estado" ? "estado_desc" : "Estado";

            var credenciales = _context.Credenciales.AsQueryable();

            // Verificar si hay algún filtro aplicado (excluyendo sortOrder)
            bool hayFiltrosAplicados = !string.IsNullOrEmpty(codigoEquipoFiltro) ||
                                         fechaYHoraDesdeFiltro.HasValue ||
                                         fechaYHoraHastaFiltro.HasValue ||
                                         !string.IsNullOrEmpty(codigoUsuarioEcarFiltro) ||
                                         !string.IsNullOrEmpty(nombreUsuarioFiltro) ||
                                         !string.IsNullOrEmpty(perfilFiltro) ||
                                         !string.IsNullOrEmpty(usuarioFiltro) ||
                                         !string.IsNullOrEmpty(estadoFiltro) ||
                                         !string.IsNullOrEmpty(usuarioRegistroFiltro) ||
                                         retirosPendientes;

            if (retirosPendientes)
            {
                var codigosEmpleadosInactivos = await _context.Empleados
                                                              .Where(e => e.Estado == "Inactivo")
                                                              .Select(e => e.CodigoEmpleadoEcar)
                                                              .ToListAsync();

                credenciales = credenciales.Where(c => c.Estado == "Activo" && codigosEmpleadosInactivos.Contains(c.CodigoUsuarioEcar));

                TempData["InfoMessage"] = $"Mostrando retiros pendientes: {await credenciales.CountAsync()} registros encontrados.";
            }
            else
            {
                // Solo aplicar filtro de "hoy" si NO hay filtros aplicados Y es la primera carga
                if (!hayFiltrosAplicados && aplicarFiltroHoy && string.IsNullOrEmpty(sortOrder))
                {
                    var hoy = DateTime.Today;
                    credenciales = credenciales.Where(c => c.FechaYHora.Date == hoy);
                    TempData["InfoMessage"] = "Mostrando registros del día de hoy.";
                }

                // Aplicar filtros individuales
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
            }

            // Switch de ordenamiento
            switch (sortOrder)
            {
                case "nombre_desc":
                    credenciales = credenciales.OrderByDescending(c => c.NombreUsuario);
                    break;
                case "CodigoEquipo":
                    credenciales = credenciales.OrderBy(c => c.CodigoEquipo);
                    break;
                case "codigoequipo_desc":
                    credenciales = credenciales.OrderByDescending(c => c.CodigoEquipo);
                    break;
                case "CodigoEmpleado":
                    credenciales = credenciales.OrderBy(c => c.CodigoUsuarioEcar);
                    break;
                case "codigoempleado_desc":
                    credenciales = credenciales.OrderByDescending(c => c.CodigoUsuarioEcar);
                    break;
                case "Estado":
                    credenciales = credenciales.OrderBy(c => c.Estado);
                    break;
                case "estado_desc":
                    credenciales = credenciales.OrderByDescending(c => c.Estado);
                    break;
                default:
                    credenciales = credenciales.OrderBy(c => c.NombreUsuario);
                    break;
            }

            var credencialesList = await credenciales.ToListAsync();

            // Lógica de desencriptación
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
            return View(credencialesList);
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

        [HttpGet]
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

        [HttpGet]
        public IActionResult Create()
        {
            var nuevaCredencial = new Credencial
            {
                FechaYHora = DateTime.Now,
                Estado = "Activo",
                UsuarioRegistro = User?.Identity?.Name
            };
            return View(nuevaCredencial);
        }

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

                // Lógica de Auditoría: Registro de creación
                await RegistrarAuditoriaAsync(User.Identity.Name, "Crear", "Credenciales", $"Se creó la credencial para el equipo '{credencial.CodigoEquipo}' y el usuario '{credencial.NombreUsuario}'.");

                TempData["SuccessMessage"] = $"La credencial para el equipo '{credencial.CodigoEquipo}' y usuario '{credencial.NombreUsuario}' ha sido creada exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            return View(credencial);
        }

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

                // Lógica de Auditoría: Registro de clonación
                await RegistrarAuditoriaAsync(User.Identity.Name, "Clonar", "Credenciales", $"Se clonó la credencial con ID {ViewBag.CredencialOriginalId} para el equipo '{credencial.CodigoEquipo}' y el usuario '{credencial.NombreUsuario}'.");

                TempData["SuccessMessage"] = "La credencial se ha clonado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View("Clonar", credencial);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var credencial = await _context.Credenciales.FirstOrDefaultAsync(m => m.Id == id);
            if (credencial == null)
            {
                return NotFound();
            }

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

        [HttpGet]
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

                    // Se registra el estado actual de la credencial antes de la actualización
                    string estadoAnterior = credencialToUpdate.Estado;

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

                    // Lógica de Auditoría: Registro de edición
                    await RegistrarAuditoriaAsync(User.Identity.Name, "Editar", "Credenciales", $"Se actualizó la credencial con ID {id} para el equipo '{credencial.CodigoEquipo}'. Se cambió el estado de '{estadoAnterior}' a '{credencial.Estado}'.");

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

            // Lógica de Auditoría: Registro de eliminación
            await RegistrarAuditoriaAsync(User.Identity.Name, "Eliminar", "Credenciales", $"Se eliminó la credencial con ID {id} para el equipo '{credencial.CodigoEquipo}' y el usuario '{credencial.NombreUsuario}'.");

            TempData["SuccessMessage"] = $"La credencial para el equipo '{credencial.CodigoEquipo}' y usuario '{credencial.NombreUsuario}' ha sido eliminada exitosamente.";
            return RedirectToAction(nameof(Index));
        }

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

                // Lógica de Auditoría: Registro de activación
                await RegistrarAuditoriaAsync(User.Identity.Name, "Activar", "Credenciales", $"Se activó la credencial con ID {id} para el equipo '{credencial.CodigoEquipo}' y el usuario '{credencial.NombreUsuario}'.");

                TempData["SuccessMessage"] = $"La credencial para el empleado '{credencial.NombreUsuario}' en el equipo '{credencial.CodigoEquipo}' ha sido activada correctamente.";
            }
            else if (credencial.Estado == "Activo")
            {
                TempData["InfoMessage"] = $"La credencial para el empleado '{credencial.NombreUsuario}' en el equipo '{credencial.CodigoEquipo}' ya se encuentra activa.";
            }

            return RedirectToAction(nameof(Index));
        }

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

            // Lógica de Auditoría: Registro de desactivación
            await RegistrarAuditoriaAsync(User.Identity.Name, "Desactivar", "Credenciales", $"Se desactivó la credencial con ID {id} para el equipo '{credencial.CodigoEquipo}' y el usuario '{credencial.NombreUsuario}'.");

            TempData["SuccessMessage"] = $"La credencial para el empleado '{credencial.NombreUsuario}' en el equipo '{credencial.CodigoEquipo}' ha sido inactivada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private bool CredencialExists(int id)
        {
            return _context.Credenciales.Any(e => e.Id == id);
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