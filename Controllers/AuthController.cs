using Eva4_AuthMVC.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EVA3.Controllers
{
    public class AuthController : Controller

    {
        private readonly AppDbContext db;

        public AuthController(AppDbContext db)
        {
            this.db = db;
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (db.Usuarios.ToList().Count == 0)
            {
                var usuario = new Usuario();
                usuario.Name = "SuperAdministrador";
                usuario.Email = "SuperAdmin@midominio.cl";
                usuario.Rol = "SuperAdministrador";
                usuario.isBlocked = false;
                CreatePasswordHash("SuperAdministrador", out byte[] hash, out byte[] salt);
                usuario.PasswordHash = hash;
                usuario.PasswordSalt = salt;
                db.Usuarios.Add(usuario);
                db.SaveChanges();
            }
            var user = db.Usuarios.FirstOrDefault(x => x.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Usuario No Encontrado");
                return View(model);
            }
            if (user.isBlocked != true) {
                if (VerifyPasswordHash(model.Password, user.PasswordSalt, user.PasswordHash))
                {
                    var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                    new Claim(ClaimTypes.Name,user.Name),
                    new Claim(ClaimTypes.Email,user.Email),
                    new Claim(ClaimTypes.Role,user.Rol),

                };//Datos del carnet
                    var Identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);//Carnet
                    var Principal = new ClaimsPrincipal(Identity);//Billetera
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, Principal,
                        new AuthenticationProperties { IsPersistent = true });
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Password Incorrecta");
                    return View(model);
                }
            }
            else
            {
                ModelState.AddModelError("", "Usuario Bloqueado");
                return View(model);
            }
            
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {

            return View();
        }



        public static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        public static bool VerifyPasswordHash(string password, byte[] passwordSalt, byte[] passwordhash)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordhash);
            }
        }

        public async Task<RedirectToActionResult> LogOut()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}
