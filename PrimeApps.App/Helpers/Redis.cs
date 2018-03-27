using System.Configuration;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Jil;

namespace PrimeApps.App.Helpers
{
    public static class Redis
    {
        private static JilSerializer _serializer = new JilSerializer();
        private static ConnectionMultiplexer _multiplexer = ConnectionMultiplexer.Connect(BuildConfig());
        private static StackExchangeRedisCacheClient _cacheClient = new StackExchangeRedisCacheClient(_multiplexer, _serializer);

        /// <summary>
        /// Provides an instance of redis client.
        /// </summary>
        /// <returns></returns>

        public static StackExchangeRedisCacheClient Client()
        {

            return _cacheClient;
        }

        /// <summary>
        /// Returns the configured multiplexer for redis clients.
        /// </summary>
        /// <returns></returns>
        public static ConnectionMultiplexer GetMultiplexer()
        {
            return _multiplexer;
        }

        /// <summary>
        /// Builds a new multiplexer configuration by using connection string.
        /// </summary>
        /// <returns></returns>
        public static ConfigurationOptions BuildConfig()
        {
            var config = ConfigurationOptions.Parse(ConfigurationManager.ConnectionStrings["RedisConnection"].ToString());
            config.ConnectTimeout = 5000;
            config.ConnectRetry = 5;
            config.SyncTimeout = 5000;

            var testMode = bool.Parse(ConfigurationManager.AppSettings["TestMode"]);

            if (!testMode)
                config.Ssl = true;

            return config;
        }
    }
}
