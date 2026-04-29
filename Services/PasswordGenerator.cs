using System.Security.Cryptography;
using System.Text;

namespace LogisticaBroker.Services
{
    /// <summary>
    /// Genera contraseñas temporales seguras (T17).
    /// Usa RandomNumberGenerator del framework — no usa Random.
    /// </summary>
    public static class PasswordGenerator
    {
        private const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        private const string Lower = "abcdefghjkmnpqrstuvwxyz";
        private const string Digits = "23456789";
        private const string Special = "@#$%&!?";

        /// <summary>
        /// Genera una contraseña temporal de longitud indicada que garantiza al menos
        /// 1 mayúscula, 1 minúscula, 1 dígito y 1 carácter especial.
        /// </summary>
        public static string Generate(int length = 12)
        {
            if (length < 8) throw new ArgumentException("Longitud mínima: 8", nameof(length));

            var all = Upper + Lower + Digits + Special;
            var password = new char[length];

            // Garantizar al menos uno de cada tipo
            password[0] = PickRandom(Upper);
            password[1] = PickRandom(Lower);
            password[2] = PickRandom(Digits);
            password[3] = PickRandom(Special);

            for (int i = 4; i < length; i++)
                password[i] = PickRandom(all);

            // Mezclar con Fisher-Yates criptográfico
            for (int i = length - 1; i > 0; i--)
            {
                int j = RandomNumberGenerator.GetInt32(i + 1);
                (password[i], password[j]) = (password[j], password[i]);
            }

            return new string(password);
        }

        private static char PickRandom(string chars) =>
            chars[RandomNumberGenerator.GetInt32(chars.Length)];
    }
}
