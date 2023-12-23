using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Eva4_AuthMVC.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Rol { get; set; }
        public bool isBlocked { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
    }
    public class UsuarioDTO
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Rol { get; set; }
        public bool isBlocked { get; set; }
 
        public string Password {  get; set; }
    }

    public class SuperAdminDTO
    {
        public string Name { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }

    }

    public class UsuarioViewDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Rol { get; set; }
    }
}