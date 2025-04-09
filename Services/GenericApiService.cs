using System.Net;
using System.Web;
using NetCoreCommonLibrary.Models;
using NetCoreCommonLibrary.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreCommonLibrary.Services
{
    /// <summary>
    /// Implementação genérica base para serviços que consomem endpoints de API RESTful para uma entidade específica.
    /// Fornece operações CRUD básicas e paginação.
    /// </summary>
    /// <typeparam name="TEntity">O tipo da entidade gerenciada pelo serviço.</typeparam>
    /// <typeparam name="TKey">O tipo da chave primária da entidade.</typeparam>
    public class GenericApiService<TEntity, TKey> : BaseApiService, IGenericApiService<TEntity, TKey> 
        where TEntity : class
    {
        // Endpoint relativo ao recurso (ex: "users", "products")
        protected readonly string _resourceEndpoint;

        /// <summary>
        /// Construtor do serviço genérico.
        /// </summary>
        /// <param name="httpClient">Instância de HttpClient configurada.</param>
        /// <param name="logger">Instância de ILogger.</param>
        /// <param name="resourceEndpoint">O caminho relativo para o recurso na API (ex: "/api/users").</param>
        public GenericApiService(
            HttpClient httpClient, 
            ILogger<GenericApiService<TEntity, TKey>> logger, // Logger específico para a classe genérica
            string resourceEndpoint) 
            : base(httpClient, logger) // Chama o construtor da BaseApiService
        {
            // Valida e armazena o endpoint do recurso, garantindo que não tenha barras extras
            if (string.IsNullOrWhiteSpace(resourceEndpoint))
            {
                throw new ArgumentException("Resource endpoint cannot be null or whitespace.", nameof(resourceEndpoint));
            }
            _resourceEndpoint = resourceEndpoint.Trim('/');
        }

        /// <inheritdoc/>
        public virtual async Task<ApiResponse<IEnumerable<TEntity>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            // Chama o método GetAsync da classe base, passando o endpoint do recurso
            return await GetAsync<IEnumerable<TEntity>>(_resourceEndpoint, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<ApiResponse<TEntity>> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
        {
            // Constrói o endpoint específico para buscar por ID
            return await GetAsync<TEntity>($"{_resourceEndpoint}/{id}", cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<ApiResponse<TEntity>> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entity);
            // Chama o método PostAsync da classe base
            // Espera que a API retorne a entidade criada como TEntity no corpo da resposta
            return await PostAsync<TEntity, TEntity>(_resourceEndpoint, entity, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<ApiResponse> UpdateAsync(TKey id, TEntity entity, CancellationToken cancellationToken = default)
        {
             ArgumentNullException.ThrowIfNull(entity);
            // Chama o método PutAsync da classe base
            return await PutAsync($"{_resourceEndpoint}/{id}", entity, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<ApiResponse> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
        {
            // Chama o método DeleteAsync da classe base
            return await DeleteAsync($"{_resourceEndpoint}/{id}", cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<ApiResponse<PaginatedList<TEntity>>> GetPagedAsync(
            int pageIndex, 
            int pageSize, 
            string? searchTerm = null, 
            string? sortColumn = null, 
            bool sortAscending = true, 
            CancellationToken cancellationToken = default)
        {
            // Validações básicas para paginação
             if (pageIndex < 1) pageIndex = 1;
             if (pageSize < 1) pageSize = 10; // Ou um valor padrão razoável
             
            // Usa UriBuilder ou HttpUtility para construir a query string de forma segura
            var queryParams = HttpUtility.ParseQueryString(string.Empty); 
            queryParams["pageIndex"] = pageIndex.ToString();
            queryParams["pageSize"] = pageSize.ToString();

            if (!string.IsNullOrWhiteSpace(searchTerm))
                queryParams["search"] = searchTerm;
            
            if (!string.IsNullOrWhiteSpace(sortColumn))
            {
                queryParams["sortColumn"] = sortColumn;
                queryParams["sortAscending"] = sortAscending.ToString().ToLowerInvariant();
            }

            string requestUrl = $"{_resourceEndpoint}?{queryParams}";

            // Chama o método GetAsync da classe base esperando uma PaginatedList<TEntity>
            return await GetAsync<PaginatedList<TEntity>>(requestUrl, cancellationToken);
            
            // O tratamento de erro já está na BaseApiService.GetAsync
        }
    }
} 