using EVA3.Controllers;
using Eva4_AuthMVC.Data;
using Eva4_AuthMVC.Models;
using Eva4_AuthMVC.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Reflection.Metadata.Ecma335;

namespace Eva4_AuthMVC.Controllers
{
    
    public class UsuarioController : Controller
    {
        private readonly AppDbContext _context;

        public UsuarioController(AppDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "SuperAdministrador, Administrador")]
        public IActionResult Index(bool error = false)
        {


            if (error)
            {
                ModelState.AddModelError("", "No se pueden realizar cambios, ni eliminar al SuperAdministrador desde otros perfiles");
            }
            var model = new FindUsuarioViewModel();
            model.Usuarios = _context.Usuarios.ToList();

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "SuperAdministrador, Administrador")]
        public IActionResult Index(FindUsuarioViewModel model)
        {

            if (model.opt==null || model.value==null)
            {   
                model.Usuarios= _context.Usuarios.ToList();
                return View(model);
            }
            else
            {
                if (model.opt == 1) { 
                
                    model.Usuarios= _context.Usuarios.Where(x => x.Name == model.value);
                    return View(model);


                }
                else if (model.opt == 2)
                {
                model.Usuarios = _context.Usuarios.Where(x => x.Email == model.value);
                    return View(model);

                }
                else if (model.opt == 3)
                {
                model.Usuarios = _context.Usuarios.Where(x => x.Rol == model.value);
                    return View(model);

                }
                else
                {
                model.Usuarios  = _context.Usuarios.ToList();
                    return View(model);
                }
            }
        }


        [Authorize(Roles = "SuperAdministrador, Administrador")]
        public IActionResult Create()
        {
            return View();
        }
        [Authorize(Roles = "SuperAdministrador, Administrador")]
        [HttpPost]
        public IActionResult Create(UsuarioDTO usuarioDTO)
        {


            if (usuarioDTO != null)
            {   
                Usuario usuario = new Usuario();
                usuario.Name = usuarioDTO.Name;
                usuario.Email = usuarioDTO.Email;
                usuario.Rol=usuarioDTO.Rol;
                usuario.isBlocked = usuarioDTO.isBlocked;
                AuthController.CreatePasswordHash(usuarioDTO.Password,out byte[] hash, out byte[] salt);
                usuario.PasswordHash = hash;
                usuario.PasswordSalt = salt;
                _context.Usuarios.Add(usuario);
                _context.SaveChanges();
                return RedirectToAction("Index", "Usuario");
            }
            else
            {
                ModelState.AddModelError("", "Error");
                return View();
            }
        }
        [Authorize(Roles = "SuperAdministrador, Administrador")]
        [HttpPost]
        public IActionResult ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Error, nuevas contraseñas no coinciden");
                return View();
            }
            var usuario= _context.Usuarios.First(u => u.Id == Int32.Parse(User.getUserId()));
            if (usuario != null)
            {
                if(AuthController.VerifyPasswordHash(oldPassword, usuario.PasswordSalt, usuario.PasswordHash))
                {
                    AuthController.CreatePasswordHash(newPassword, out byte[] hash, out byte[] salt);
                    usuario.PasswordHash=hash;
                    usuario.PasswordSalt=salt;
                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Error contraseña actual incorrecta");
                    return View();
                }
            }
            else
            {
                ModelState.AddModelError("", "Error");
                return View();
            }
        }
        [Authorize(Roles = "SuperAdministrador, Administrador")]
        public IActionResult ChangePassword()
        {
            return View();
        }
        [Authorize(Roles = "SuperAdministrador, Administrador,Vendedor,Supervisor")]
        public IActionResult ViewPerfil()

        {
                UsuarioViewDTO usuarioViewDTO=new UsuarioViewDTO();
                usuarioViewDTO.Id = Int32.Parse(User.getUserId());
                usuarioViewDTO.Email=User.getUserEmail();
                usuarioViewDTO.Name = User.getUserName();
                usuarioViewDTO.Rol = User.getUserRol();
                return View(usuarioViewDTO);

        }

        [Authorize(Roles = "SuperAdministrador, Administrador")]
        public IActionResult ToggleBlocked(int Id)
        {
            var user=_context.Usuarios.FirstOrDefault(x=>x.Id==Id);
            if (user.Rol == "SuperAdministrador")
            {
                
                return RedirectToAction("Index", new { error = true });
            }
            if (user != null)
            {
                user.isBlocked=!user.isBlocked;
                _context.Update(user);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "Ha ocurrido un error, vuelve a intentar");
                return RedirectToAction("Index");
            }

           
        }
        [Authorize(Roles = "SuperAdministrador, Administrador")]
        public IActionResult Edit(int Id)
        {
            var user=_context.Usuarios.FirstOrDefault(x=>x.Id == Id);
            if (User.getUserRol()== "SuperAdministrador"&& user.Rol== "SuperAdministrador")

            {
                return RedirectToAction("EditSuperAdmin", new { id =Id});
            }

            if ((user.Rol == "SuperAdministrador"))
            {
                return RedirectToAction("Index", new { error = true });
            }
            if (user != null)
            {
                var dto=new UsuarioDTO();
                dto.Email = user.Email;
                dto.Name=user.Name;
                dto.Rol = user.Rol;
                dto.isBlocked = user.isBlocked;
                var model = new EditViewModel();
                model.dto = dto;
                model.Id = Id;
                return View(model);
            }
            else
            {
                return NotFound();
            }
        }
        [Authorize(Roles = "SuperAdministrador, Administrador")]
        [HttpPost]

        public IActionResult Edit(EditViewModel model)
        {
            
            if (model.dto.Password == null)
            {
                var usuario=_context.Usuarios.FirstOrDefault(u=>u.Id == model.Id);
                usuario.Email = model.dto.Email;
                usuario.Name=model.dto.Name;
                usuario.Rol = model.dto.Rol;
                usuario.isBlocked = model.dto.isBlocked;
                _context.Update(usuario); 
                _context.SaveChanges(); 
                return RedirectToAction("Index");
            }
            else
            {
                var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == model.Id);
                usuario.Email = model.dto.Email;
                usuario.Name = model.dto.Name;
                usuario.Rol = model.dto.Rol;
                usuario.isBlocked = model.dto.isBlocked;
                AuthController.CreatePasswordHash(model.dto.Password,out byte[] hash, out byte[] salt);
                usuario.PasswordHash = hash;
                usuario.PasswordSalt = salt;
                _context.Update(usuario); _context.SaveChanges(); return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "SuperAdministrador, Administrador")]
        public IActionResult EditSuperAdmin(int Id)
        {
            var user = _context.Usuarios.FirstOrDefault(x => x.Id == Id);

            if (user != null)
            {
                var super = new SuperAdminDTO();
                super.Email = user.Email;
                super.Name = user.Name;
                var modelSuper = new EditSuperAdminViewModel();
                modelSuper.id = Id;
                modelSuper.SuperAdmin = super;
                return View(modelSuper);

            }
            else
            {
                ModelState.AddModelError("", "Ha ocurrido un error, vuelve a intentar");
                return RedirectToAction("Index");
            }

        }


        [HttpPost]
        [Authorize(Roles = "SuperAdministrador, Administrador")]
        public IActionResult EditSuperAdmin(EditSuperAdminViewModel model)
        {
            if (model.SuperAdmin.Password == null)
            {
                var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == model.id);
                usuario.Email = model.SuperAdmin.Email;
                usuario.Name = model.SuperAdmin.Name;
                _context.Update(usuario);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == model.id);
                usuario.Email = model.SuperAdmin.Email;
                usuario.Name = model.SuperAdmin.Name;
                AuthController.CreatePasswordHash(model.SuperAdmin.Password, out byte[] hash, out byte[] salt);
                usuario.PasswordHash = hash;
                usuario.PasswordSalt = salt;
                _context.Update(usuario); _context.SaveChanges(); return RedirectToAction("Index");
            }
        }
        [Authorize(Roles = "SuperAdministrador, Administrador")]
        public IActionResult Delete (int id)
        {
            var usuario = _context.Usuarios.Find(id);
            if (usuario.Rol == "SuperAdministrador")
            {

                return RedirectToAction("Index", new { error = true });
            }
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }return NotFound();

        }

    }
}
