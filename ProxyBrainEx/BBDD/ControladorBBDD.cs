using Dapper;
using ProxyBrainEx.Models;

namespace ProxyBrainEx.BBDD
{
    public class ControladorBBDD
    {
        private readonly ClienteBBDD _clienteBBDD;

        public ControladorBBDD()
        {
            _clienteBBDD = new ClienteBBDD();
        }

        public async Task<bool> InsertarUsuarioAsync(UsuarioRegistro usuario)
        {
            const string sql = @"
				INSERT INTO usuarios (guid_id, nombre, usuario, email, contrasena)
				VALUES (@GuidId, @Nombre, @NombreUsuario, @Email, @Contrasena);";

            try
            {
                using var conexion = _clienteBBDD.ObtenerConexion();
                var parametros = new
                {
                    GuidId = Guid.NewGuid().ToString(),
                    Nombre = usuario.Nombre,
                    NombreUsuario = usuario.Usuario,
                    Email = usuario.Email,
                    Contrasena = usuario.Contrasena // En producción, hashea antes
                };

                var filas = await conexion.ExecuteAsync(sql, parametros);
                return filas > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al insertar usuario: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> UsuarioExisteAsync(string nombreUsuario, string email)
        {
            const string sql = "SELECT COUNT(*) FROM usuarios WHERE usuario = @Usuario OR email = @Email";

            try
            {
                using var conexion = _clienteBBDD.ObtenerConexion();
                int count = await conexion.ExecuteScalarAsync<int>(sql, new { Usuario = nombreUsuario, Email = email });
                return count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al verificar existencia de usuario/email: {ex.Message}");
                return true; // Por seguridad, asumimos que existe si hay error
            }
        }
        public async Task<UsuarioRegistro?> ObtenerUsuarioPorNombreOEmailAsync(string usuarioOEmail)
        {
            const string sql = @"
				SELECT nombre, usuario, email, contrasena
				FROM usuarios
				WHERE usuario = @Valor OR email = @Valor
				LIMIT 1;";

            try
            {
                using var conexion = _clienteBBDD.ObtenerConexion();
                return await conexion.QueryFirstOrDefaultAsync<UsuarioRegistro>(sql, new { Valor = usuarioOEmail });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener usuario: {ex.Message}");
                return null;
            }
        }

    }
}
