using System;
using AcmeEncuestasWeb.Data;
using AcmeEncuestasWeb.Models;

namespace AcmeEncuestasWeb.Services
{
    /// <summary>
    /// Capa de Servicio de Autenticación.
    /// Contiene la lógica de negocio para validar credenciales y orquestar el login.
    /// </summary>
    public class AuthService
    {
        private readonly UsuarioRepository _usuarioRepository;

        public AuthService()
        {
            _usuarioRepository = new UsuarioRepository();
        }

        public AuthService(UsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        /// <summary>
        /// Valida el usuario y contraseña contra la base de datos.
        /// Si es válido, actualiza el último login y retorna la entidad Usuario; de lo contrario retorna null.
        /// </summary>
        public Usuario ValidarCredenciales(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            // 1. Obtener datos del usuario por Username
            var usuario = _usuarioRepository.ObtenerPorUsername(username);
            if (usuario == null)
                return null;

            // 2. Verificar el password usando el salt y hash almacenados (PBKDF2)
            bool esValido = PasswordHasher.VerificarPassword(password, usuario.PasswordSalt, usuario.PasswordHash);
            if (!esValido)
                return null;

            // 3. Registrar auditoría de último login
            _usuarioRepository.RegistrarUltimoLogin(usuario.UsuarioId);

            return usuario;
        }
    }
}
