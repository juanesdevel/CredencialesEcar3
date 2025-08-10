using ECARTemplate.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ECARTemplate.Controllers
{
    // El atributo [Authorize] sin parámetros utilizará el esquema por defecto
    // que configuramos en Startup.cs (que ahora es la autenticación de Windows).
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Esta acción de Index() requerirá que el usuario esté autenticado
        public IActionResult Index()
        {
            return View();
        }

        // Si quieres que la página de privacidad no requiera autenticación,
        // puedes usar el atributo [AllowAnonymous].
        // [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // La acción de Error() no necesita autenticación
        [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}