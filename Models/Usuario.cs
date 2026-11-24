using System;

namespace AcmeEncuestasWeb.Models
{
    /// <summary>
    /// Entidad que representa la tabla de Usuarios en base de datos.
    /// </summary>
    public class Usuario
    {
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public bool EsActivo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimoLogin { get; set; }
    }
}
