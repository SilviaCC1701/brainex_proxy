using Dapper;
using MySql.Data.MySqlClient;
using System.Data;
using ProxyBrainEx.Config;

namespace ProxyBrainEx.BBDD
{
    public class ClienteBBDD
    {
        private readonly string _connectionString;

        public ClienteBBDD()
        {
            _connectionString = ConfigProxy.ConnectionString;
        }

        public IDbConnection ObtenerConexion()
        {
            return new MySqlConnection(_connectionString);
        }
    }
}
