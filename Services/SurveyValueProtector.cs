using System;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;

namespace AcmeEncuestasWeb.Services
{
    public static class SurveyValueProtector
    {
        private const string Prefix = "enc:v1:";
        private static readonly string[] Purposes = { "AcmeEncuestas", "ValoresCampo", "v1" };

        public static string Protect(string value)
        {
            if (value == null)
            {
                return null;
            }

            var protectedValue = MachineKey.Protect(Encoding.UTF8.GetBytes(value), Purposes);
            if (protectedValue == null || protectedValue.Length == 0)
            {
                throw new CryptographicException("No fue posible proteger el valor de la respuesta.");
            }

            return Prefix + Convert.ToBase64String(protectedValue);
        }

        public static string Unprotect(string value, byte formato)
        {
            if (formato == 0)
            {
                return value;
            }

            if (formato != 1)
            {
                throw new CryptographicException("La versión del valor cifrado no es compatible.");
            }

            if (value == null)
            {
                throw new CryptographicException("El valor cifrado está vacío.");
            }

            if (!value.StartsWith(Prefix, StringComparison.Ordinal))
            {
                throw new CryptographicException("El valor cifrado está dañado.");
            }

            byte[] protectedValue;
            try
            {
                protectedValue = Convert.FromBase64String(value.Substring(Prefix.Length));
            }
            catch (FormatException ex)
            {
                throw new CryptographicException("El valor cifrado está dañado.", ex);
            }

            var plaintext = MachineKey.Unprotect(protectedValue, Purposes);
            if (plaintext == null)
            {
                throw new CryptographicException("No fue posible validar o descifrar el valor de la respuesta.");
            }

            return Encoding.UTF8.GetString(plaintext);
        }
    }
}
