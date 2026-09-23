using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace VinotecaApp.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            // Si ya está logueado, lo mandamos al panel
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Ventas"); // O a "Ventas"
                
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string usuario, string password)
        {
            // ACÁ DEFINÍS EL USUARIO Y CONTRASEÑA DE TU PAPÁ
            if (usuario == "leandro" && password == "empatia2026")
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario)
                };

                var identity = new ClaimsIdentity(claims, "CookieAuth");
                var principal = new ClaimsPrincipal(identity);

                // Iniciamos sesión
                await HttpContext.SignInAsync("CookieAuth", principal);

                return RedirectToAction("Index", "Ventas"); // Lo mandamos al panel
            }

            // Si le pifia a la clave
            ViewBag.Error = "Usuario o contraseña incorrectos";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            // Esto borra la cookie y cierra la sesión
            await HttpContext.SignOutAsync("CookieAuth");
    
            // Lo mandamos de vuelta a la pantalla de Login
            return RedirectToAction("Login", "Account"); 
        }
    }
}