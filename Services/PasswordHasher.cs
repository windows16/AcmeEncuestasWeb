using System;
using System.Security.Cryptography;

namespace AcmeEncuestasWeb.Services
{
    /// <summary>
    /// Servicio de Criptografía que implementa PBKDF2 (Rfc2898DeriveBytes) con HMAC-SHA256.
    /// Cumple con los puntos 4.1 y 4.2 del examen técnico:
    /// - Algoritmo estándar de la industria resistente a ataques de diccionario y Rainbow Tables.
    /// - Salting aleatorio criptográficamente fuerte para cada usuario.
    /// - Comparación segura de tiempo constante para prevenir Timing Attacks.
    /// </summary>
    public static class PasswordHasher
    {
        private const int SaltByteSize = 32;       // 256 bits de salt
        private const int HashByteSize = 32;       // 256 bits de subclave
        private const int IterationsCount = 10000; // Estándar NIST para PBKDF2

        /// <summary>
        /// Genera un Salt criptográficamente seguro codificado en Base64.
        /// </summary>
        public static string GenerarSalt()
        {
            byte[] salt = new byte[SaltByteSize];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            return Convert.ToBase64String(salt);
        }

        /// <summary>
        /// Genera el Hash de una contraseña plana utilizando el Salt provisto.
        /// </summary>
        public static string HashearPassword(string password, string saltBase64)
        {
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrEmpty(saltBase64)) throw new ArgumentNullException(nameof(saltBase64));

            byte[] saltBytes = Convert.FromBase64String(saltBase64);

            // Rfc2898DeriveBytes con SHA256 disponible nativamente en .NET Framework
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, IterationsCount, HashAlgorithmName.SHA256))
            {
                byte[] hashBytes = pbkdf2.GetBytes(HashByteSize);
                return Convert.ToBase64String(hashBytes);
            }
        }

        /// <summary>
        /// Verifica si la contraseña ingresada coincide con el hash almacenado.
        /// Utiliza comparación de longitud fija en tiempo constante para evitar Timing Attacks.
        /// </summary>
        public static bool VerificarPassword(string passwordIngresada, string saltBase64, string hashAlmacenadoBase64)
        {
            if (string.IsNullOrEmpty(passwordIngresada) || string.IsNullOrEmpty(saltBase64) || string.IsNullOrEmpty(hashAlmacenadoBase64))
                return false;

            string nuevoHash = HashearPassword(passwordIngresada, saltBase64);

            byte[] a = Convert.FromBase64String(nuevoHash);
            byte[] b = Convert.FromBase64String(hashAlmacenadoBase64);

            return ComparacionTiempoConstante(a, b);
        }

        private static bool ComparacionTiempoConstante(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return false;

            int diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }
            return diff == 0;
        }
    }
}
