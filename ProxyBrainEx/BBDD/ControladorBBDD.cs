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
        public async Task<UsuarioBBDD?> ObtenerUsuarioPorNombreOEmailAsync(string usuarioOEmail)
        {
            const string sql = @"
				SELECT nombre, usuario, email, contrasena, guid_id
				FROM usuarios
				WHERE usuario = @Valor OR email = @Valor
				LIMIT 1;";

            try
            {
                using var conexion = _clienteBBDD.ObtenerConexion();
                return await conexion.QueryFirstOrDefaultAsync<UsuarioBBDD>(sql, new { Valor = usuarioOEmail });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener usuario: {ex.Message}");
                return null;
            }
        }
        public async Task<bool> InsertarEstadisticaCalculoRapidoAsync(EstadisticaCalculoRapido estadistica)
        {
            const string sql = @"
                INSERT INTO calculo_rapido (user_guid, timestamp_utc, raw_data)
                VALUES (@GuidUsuario, @Timestamp_Utc, @Raw_Data);";

            try
            {
                using var conexion = _clienteBBDD.ObtenerConexion();
                var filas = await conexion.ExecuteAsync(sql, estadistica);
                return filas > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al insertar estadística: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> InsertarEstadisticaGenericaAsync(string tabla, string guid, DateTime timestamp, object rawData)
        {
            string sql = $@"
                INSERT INTO {tabla} (user_guid, timestamp_utc, raw_data)
                VALUES (@Guid, @Timestamp, @Raw_Data);";

            try
            {
                using var conexion = _clienteBBDD.ObtenerConexion();
                var filas = await conexion.ExecuteAsync(sql, new
                {
                    Guid = guid,
                    Timestamp = timestamp,
                    RawData = System.Text.Json.JsonSerializer.Serialize(rawData)
                });
                return filas > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al insertar en {tabla}: {ex.Message}");
                return false;
            }
        }

        public async Task<UsuarioBBDD?> GetUserByGuid(string guid)
        {
            const string sql =
              @"SELECT nombre, usuario, email, guid_id 
				FROM usuarios 
				WHERE guid_id = @GuidID;";

            try
            {
                using var conexion = _clienteBBDD.ObtenerConexion();
                return await conexion.QueryFirstOrDefaultAsync<UsuarioBBDD>(sql, new { GuidID = guid });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener usuario '{guid}': {ex.Message}");
                return null;
            }
        }

        public async Task<List<PartidaItemBBDD>> ObtenerPartidasPorUsuarioAsync(string guid)
        {
            var sql = @"
                SELECT id, user_guid, timestamp_utc, raw_data, 'calculo_rapido' AS tipo FROM calculo_rapido WHERE user_guid = @Guid
                UNION ALL
                SELECT id, user_guid, timestamp_utc, raw_data, 'completa_operacion' AS tipo FROM completa_operacion WHERE user_guid = @Guid
                UNION ALL
                SELECT id, user_guid, timestamp_utc, raw_data, 'encuentra_patron' AS tipo FROM encuentra_patron WHERE user_guid = @Guid
                UNION ALL
                SELECT id, user_guid, timestamp_utc, raw_data, 'sigue_secuencia' AS tipo FROM sigue_secuencia WHERE user_guid = @Guid
                UNION ALL
                SELECT id, user_guid, timestamp_utc, raw_data, 'memory_game' AS tipo FROM memory_game WHERE user_guid = @Guid
                UNION ALL
                SELECT id, user_guid, timestamp_utc, raw_data, 'torre_hanoi' AS tipo FROM torre_hanoi WHERE user_guid = @Guid
                ORDER BY timestamp_utc DESC;
            ";

            try
            {
                using var conexion = _clienteBBDD.ObtenerConexion();
                var resultados = await conexion.QueryAsync<PartidaItemBBDD>(sql, new { Guid = guid });
                return resultados.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ObtenerPartidasPorUsuarioAsync: {ex.Message}");
                return new List<PartidaItemBBDD>();
            }
        }
    }
}
