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
    // Este controlador ahora es obsoleto para la autenticaci�n
    // pero a�n puede contener otras acciones p�blicas.
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
            // El logout ya no se hace con un esquema de autenticaci�n espec�fico
            // El usuario simplemente cierra la sesi�n de Windows en el navegador
            // Redirige a la p�gina principal.
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Redirige al Home porque el login ahora es autom�tico por AD
            return RedirectToAction("Index", "Home");
        }

        // Ya no se necesita una acci�n de login [HttpPost]
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Login(LoginViewModel model)
        // {
        //     // C�digo de autenticaci�n de base de datos eliminado.
        //     // ...
        // }

        [HttpGet]
        public IActionResult Register()
        {
            // El registro de usuarios ya no se realiza por la aplicaci�n
            // Retorna a una p�gina de acceso denegado o al inicio
            return RedirectToAction("AccessDenied");
        }

        // Ya no se necesita una acci�n de registro [HttpPost]
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Register(RegisterViewModel model)
        // {
        //     // C�digo de registro de base de datos eliminado.
        //     // ...
        // }

        public IActionResult AccessDenied() => View();

        public IActionResult Search() => View();
    }
}