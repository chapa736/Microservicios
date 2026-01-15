using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Seguros.Core.Interfaces.Application;
using System.Text.Json;
using Serilog;

namespace Seguros.Infrastructure.Services
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions;

        public CacheService(IDistributedCache cache, IConfiguration configuration)
        {
            _cache = cache;
            _configuration = configuration;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                var cachedValue = await _cache.GetStringAsync(key, cancellationToken);
                
                if (string.IsNullOrEmpty(cachedValue))
                {
                    Log.Debug("Cache miss para la clave: {Key}", key);
                    return null;
                }

                Log.Debug("Cache hit para la clave: {Key}", key);
                return JsonSerializer.Deserialize<T>(cachedValue, _jsonOptions);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error al obtener del caché la clave: {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                var jsonValue = JsonSerializer.Serialize(value, _jsonOptions);
                var options = new DistributedCacheEntryOptions();

                if (expiration.HasValue)
                {
                    options.AbsoluteExpirationRelativeToNow = expiration.Value;
                }
                else
                {
                    // Usar expiración por defecto de configuración
                    var defaultExpiration = _configuration.GetValue<int>("CacheSettings:DefaultExpirationMinutes", 5);
                    options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(defaultExpiration);
                }

                await _cache.SetStringAsync(key, jsonValue, options, cancellationToken);
                Log.Debug("Valor almacenado en caché para la clave: {Key}, expiración: {Expiration}", key, options.AbsoluteExpirationRelativeToNow);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error al almacenar en caché la clave: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                await _cache.RemoveAsync(key, cancellationToken);
                Log.Debug("Clave eliminada del caché: {Key}", key);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error al eliminar del caché la clave: {Key}", key);
            }
        }
    }
}
