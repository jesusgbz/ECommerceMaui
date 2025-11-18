using System.Security.Cryptography;
using System.Text;

namespace ECommerceMaui.Helpers
{
    public static class PasswordHelper
    {
        /// <summary>
        /// Cifra una contraseña de texto plano usando SHA-256.
        /// </summary>
        /// <param name="password">La contraseña en texto plano.</param>
        /// <returns>La contraseña cifrada como un string hexadecimal.</returns>
        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                // Convierte el string en bytes
                byte[] bytes = Encoding.UTF8.GetBytes(password);

                // Calcula el hash
                byte[] hashBytes = sha256.ComputeHash(bytes);

                // Convierte el hash (bytes) de nuevo a un string hexadecimal
                var sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2")); // "x2" formatea como hexadecimal
                }
                return sb.ToString();
            }
        }
    }
}