using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreCommonLibrary.Caching
{
    /// <summary>
    /// Extensões para IDistributedCache que facilitam o armazenamento e recuperação de objetos
    /// com serialização JSON automática.
    /// </summary>
    public static class DistributedCacheExtensions
    {
        private static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Obtém um objeto do cache distribuído, desserializando-o de JSON.
        /// </summary>
        /// <typeparam name="T">O tipo do objeto a ser recuperado.</typeparam>
        /// <param name="cache">A instância de IDistributedCache.</param>
        /// <param name="key">A chave do cache.</param>
        /// <param name="options">Opções de serialização JSON opcionais.</param>
        /// <param name="cancellationToken">Token de cancelamento opcional.</param>
        /// <returns>O objeto desserializado, ou null se não encontrado no cache.</returns>
        public static async Task<T?> GetObjectAsync<T>(
            this IDistributedCache cache,
            string key,
            JsonSerializerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var jsonBytes = await cache.GetAsync(key, cancellationToken);
            if (jsonBytes == null || jsonBytes.Length == 0)
            {
                return default;
            }

            var json = Encoding.UTF8.GetString(jsonBytes);
            return JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
        }

        /// <summary>
        /// Armazena um objeto no cache distribuído, serializando-o para JSON.
        /// </summary>
        /// <typeparam name="T">O tipo do objeto a ser armazenado.</typeparam>
        /// <param name="cache">A instância de IDistributedCache.</param>
        /// <param name="key">A chave do cache.</param>
        /// <param name="value">O objeto a ser armazenado.</param>
        /// <param name="options">Opções de cache distribuído.</param>
        /// <param name="serializerOptions">Opções de serialização JSON opcionais.</param>
        /// <param name="cancellationToken">Token de cancelamento opcional.</param>
        /// <returns>Um task que representa a operação assíncrona.</returns>
        public static async Task SetObjectAsync<T>(
            this IDistributedCache cache,
            string key,
            T value,
            DistributedCacheEntryOptions? options = null,
            JsonSerializerOptions? serializerOptions = null,
            CancellationToken cancellationToken = default)
        {
            if (value == null)
            {
                await cache.RemoveAsync(key, cancellationToken);
                return;
            }

            var json = JsonSerializer.Serialize(value, serializerOptions ?? DefaultOptions);
            var jsonBytes = Encoding.UTF8.GetBytes(json);

            await cache.SetAsync(key, jsonBytes, options ?? new DistributedCacheEntryOptions(), cancellationToken);
        }

        /// <summary>
        /// Obtém um objeto do cache ou, se não encontrado, adiciona-o usando a função de fábrica.
        /// </summary>
        /// <typeparam name="T">O tipo do objeto a ser recuperado ou armazenado.</typeparam>
        /// <param name="cache">A instância de IDistributedCache.</param>
        /// <param name="key">A chave do cache.</param>
        /// <param name="factory">A função de fábrica para criar o objeto se não encontrado no cache.</param>
        /// <param name="options">Opções de cache distribuído.</param>
        /// <param name="serializerOptions">Opções de serialização JSON opcionais.</param>
        /// <param name="cancellationToken">Token de cancelamento opcional.</param>
        /// <returns>O objeto do cache ou recém-criado.</returns>
        public static async Task<T> GetOrCreateAsync<T>(
            this IDistributedCache cache,
            string key,
            Func<Task<T>> factory,
            DistributedCacheEntryOptions? options = null,
            JsonSerializerOptions? serializerOptions = null,
            CancellationToken cancellationToken = default)
        {
            var result = await cache.GetObjectAsync<T>(key, serializerOptions, cancellationToken);
            if (result != null)
            {
                return result;
            }

            result = await factory();
            
            if (result != null)
            {
                await cache.SetObjectAsync(key, result, options, serializerOptions, cancellationToken);
            }
            
            return result!;
        }

        /// <summary>
        /// Cria opções de cache com tempo de expiração relativo.
        /// </summary>
        /// <param name="slidingExpiration">Tempo de expiração deslizante opcional.</param>
        /// <param name="absoluteExpiration">Tempo de expiração absoluto opcional.</param>
        /// <param name="absoluteExpirationRelativeToNow">Tempo de expiração absoluto relativo ao momento atual opcional.</param>
        /// <returns>As opções de cache distribuído.</returns>
        public static DistributedCacheEntryOptions CreateCacheOptions(
            TimeSpan? slidingExpiration = null,
            DateTimeOffset? absoluteExpiration = null,
            TimeSpan? absoluteExpirationRelativeToNow = null)
        {
            var options = new DistributedCacheEntryOptions();
            
            if (slidingExpiration.HasValue)
            {
                options.SlidingExpiration = slidingExpiration.Value;
            }

            if (absoluteExpiration.HasValue)
            {
                options.AbsoluteExpiration = absoluteExpiration.Value;
            }

            if (absoluteExpirationRelativeToNow.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow.Value;
            }

            return options;
        }

        /// <summary>
        /// Remove múltiplos itens do cache com base em um prefixo de chave.
        /// Requer uma implementação que suporte enumeração de chaves, como Redis.
        /// </summary>
        /// <param name="cache">A instância de IDistributedCache.</param>
        /// <param name="keyPrefix">O prefixo de chave para corresponder.</param>
        /// <returns>O número de itens removidos, se suportado pela implementação.</returns>
        /// <remarks>
        /// Esta implementação apenas demonstra a interface. Para Redis ou outro armazenamento,
        /// seria necessário implementar a funcionalidade completa.
        /// </remarks>
        public static async Task<int> RemoveByPrefixAsync(this IDistributedCache cache, string keyPrefix)
        {
            // Implementação real para Redis usando Lua script ou outras caches
            // que suportam enumeração de chaves
            
            // Este é um stub - uma implementação real precisa acessar diretamente
            // a conexão subjacente do cache específico (Redis, SQL Server, etc)
            await Task.CompletedTask;
            return 0;
        }

        /// <summary>
        /// Cria uma chave de cache conveniente baseada em tipo e parâmetros.
        /// </summary>
        /// <typeparam name="T">O tipo do objeto de cache.</typeparam>
        /// <param name="prefix">Um prefixo opcional para a chave.</param>
        /// <param name="parameters">Parâmetros opcionais a serem incluídos na chave.</param>
        /// <returns>Uma chave de cache formatada.</returns>
        public static string CreateCacheKey<T>(string? prefix = null, params object[] parameters)
        {
            var typeName = typeof(T).Name;
            if (string.IsNullOrEmpty(prefix))
            {
                prefix = typeName;
            }
            else
            {
                prefix = $"{prefix}:{typeName}";
            }

            if (parameters == null || parameters.Length == 0)
            {
                return prefix;
            }

            var paramString = string.Join(":", parameters);
            return $"{prefix}:{paramString}";
        }
    }
} 