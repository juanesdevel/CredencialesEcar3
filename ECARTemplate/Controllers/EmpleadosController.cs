using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using ECARTemplate.Models;
using ECARTemplate.Data;
using Microsoft.AspNetCore.Authorization;
using System;
using Microsoft.Data.SqlClient; // Necesario para SqlParameter

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
        public async Task<IActionResult> Index(string searchString, string estadoFilter)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["EstadoFilter"] = estadoFilter;

            var empleados = from e in _context.Empleados
                            select e;

            if (!string.IsNullOrEmpty(searchString))
            {
                empleados = empleados.Where(e =>
                    e.CodigoEmpleadoEcar.Contains(searchString) ||
                    e.NombreEmpleado.Contains(searchString));
            }

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

            var empleadosList = await empleados.ToListAsync();
            ViewData["TotalRegistros"] = empleadosList.Count;

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

        // --- ¡ACCIÓN GET 'Activar' ELIMINADA! ---
        // public async Task<IActionResult> Activar(int? id) { ... }

        // POST: Empleados/Activar/5 (Acción que ejecuta la activación y la cascada)
        [HttpPost, ActionName("Activar")] // Asegura que este POST responde a la URL /Empleados/Activar
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

                var connection = _context.Database.GetDbConnection();
                var spOutput = new SpResult { Success = 0, Message = "Error desconocido al ejecutar SP." };

                try
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "dbo.SP_Empleados_ActualizarEstado_Cascade";
                        command.CommandType = System.Data.CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@IdEmpleado", empleado.Id));
                        command.Parameters.Add(new SqlParameter("@NuevoEstado", empleado.Estado));

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
                    TempData["ErrorMessage"] = $"Empleado activado, pero hubo un error en la cascada de credenciales: {spOutput.Message}";
                }
                else
                {
                    TempData["SuccessMessage"] = $"Empleado '{empleado.NombreEmpleado}' activado exitosamente.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al activar empleado o sus credenciales: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // --- ¡ACCIÓN GET 'Desactivar' ELIMINADA! ---
        // public async Task<IActionResult> Desactivar(int? id) { ... }

        // POST: Empleados/Desactivar/5 (Acción que ejecuta la desactivación y la cascada)
        [HttpPost, ActionName("Desactivar")] // Asegura que este POST responde a la URL /Empleados/Desactivar
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Solo Admin puede desactivar
        public async Task<IActionResult> DesactivarConfirmado(int id)
        {
            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null)
            {
                TempData["ErrorMessage"] = "El empleado que intentó desactivar no fue encontrado.";
                return NotFound();
            }

            empleado.Estado = "Inactivo";

            try
            {
                _context.Update(empleado);
                await _context.SaveChangesAsync();

                var connection = _context.Database.GetDbConnection();
                var spOutput = new SpResult { Success = 0, Message = "Error desconocido al ejecutar SP." };

                try
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "dbo.SP_Empleados_ActualizarEstado_Cascade";
                        command.CommandType = System.Data.CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@IdEmpleado", empleado.Id));
                        command.Parameters.Add(new SqlParameter("@NuevoEstado", empleado.Estado));

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
                    TempData["ErrorMessage"] = $"Empleado inactivado, pero hubo un error en la cascada de credenciales: {spOutput.Message}";
                }
                else
                {
                    TempData["SuccessMessage"] = $"Empleado '{empleado.NombreEmpleado}' inactivado exitosamente.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al inactivar empleado o sus credenciales: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EmpleadoExists(int id)
        {
            return _context.Empleados.Any(e => e.Id == id);
        }
    }

    public class SpResult
    {
        public int Success { get; set; }
        public string Message { get; set; }
    }
}