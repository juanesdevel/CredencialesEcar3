using Microsoft.AspNetCore.Mvc;
using ECARTemplate.Data;
using ECARTemplate.Models;
using ECARTemplate.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Negotiate;

namespace ECARTemplate.Controllers
{
    // Este controlador ahora es obsoleto para la autenticación
    // pero aún puede contener otras acciones públicas.
    [AllowAnonymous]
    public class PageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PageController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // El logout ya no se hace con un esquema de autenticación específico
            // El usuario simplemente cierra la sesión de Windows en el navegador
            // Redirige a la página principal.
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Redirige al Home porque el login ahora es automático por AD
            return RedirectToAction("Index", "Home");
        }

        // Ya no se necesita una acción de login [HttpPost]
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Login(LoginViewModel model)
        // {
        //     // Código de autenticación de base de datos eliminado.
        //     // ...
        // }

        [HttpGet]
        public IActionResult Register()
        {
            // El registro de usuarios ya no se realiza por la aplicación
            // Retorna a una página de acceso denegado o al inicio
            return RedirectToAction("AccessDenied");
        }

        // Ya no se necesita una acción de registro [HttpPost]
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Register(RegisterViewModel model)
        // {
        //     // Código de registro de base de datos eliminado.
        //     // ...
        // }

        public IActionResult AccessDenied() => View();

        public IActionResult Search() => View();
    }
}