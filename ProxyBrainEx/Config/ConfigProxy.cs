namespace ProxyBrainEx.Config
{
    public static class ConfigProxy
    {
        private static IConfiguration _configuration;

        public static void Init(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static string Get(string key)
        {
            var envValue = Environment.GetEnvironmentVariable(key);
            return !string.IsNullOrEmpty(envValue)
                ? envValue
                : _configuration[key];
        }

        public static string ConnectionString => Get("DB_CONNECTION_STRING");
    }
}
