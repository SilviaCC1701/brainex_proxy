namespace ProxyBrainEx.Utils
{
    public static class Utilidades
    {
        public static string HashearContrasena(string contrasena)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(contrasena);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

    }
}
