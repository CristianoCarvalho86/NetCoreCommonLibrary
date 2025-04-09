using NetCoreCommonLibrary.Models;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreCommonLibrary.Services.Interfaces
{
    /// <summary>
    /// Interface genérica para serviços que interagem com endpoints de API RESTful para uma entidade específica.
    /// Define operações CRUD básicas e de paginação.
    /// </summary>
    /// <typeparam name="TEntity">O tipo da entidade gerenciada.</typeparam>
    /// <typeparam name="TKey">O tipo da chave primária da entidade.</typeparam>
    public interface IGenericApiService<TEntity, TKey> where TEntity : class
    {
        /// <summary>
        /// Obtém todos os itens da entidade de forma assíncrona.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Um ApiResponse contendo a coleção de entidades.</returns>
        Task<ApiResponse<IEnumerable<TEntity>>> GetAllAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtém um item da entidade pelo seu ID de forma assíncrona.
        /// </summary>
        /// <param name="id">O ID da entidade.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Um ApiResponse contendo a entidade ou um erro se não encontrada.</returns>
        Task<ApiResponse<TEntity>> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Cria um novo item da entidade de forma assíncrona.
        /// </summary>
        /// <param name="entity">A entidade a ser criada.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Um ApiResponse contendo a entidade criada (se a API retornar) ou um erro.</returns>
        Task<ApiResponse<TEntity>> CreateAsync(TEntity entity, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Atualiza um item existente da entidade de forma assíncrona.
        /// </summary>
        /// <param name="id">O ID da entidade a ser atualizada.</param>
        /// <param name="entity">A entidade com os dados atualizados.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Um ApiResponse indicando sucesso ou falha na atualização.</returns>
        Task<ApiResponse> UpdateAsync(TKey id, TEntity entity, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Exclui um item da entidade pelo seu ID de forma assíncrona.
        /// </summary>
        /// <param name="id">O ID da entidade a ser excluída.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Um ApiResponse indicando sucesso ou falha na exclusão.</returns>
        Task<ApiResponse> DeleteAsync(TKey id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtém uma lista paginada de itens da entidade de forma assíncrona, com opções de busca e ordenação.
        /// </summary>
        /// <param name="pageIndex">Índice da página (baseado em 1).</param>
        /// <param name="pageSize">Número de itens por página.</param>
        /// <param name="searchTerm">Termo de busca (opcional).</param>
        /// <param name="sortColumn">Nome da coluna para ordenação (opcional).</param>
        /// <param name="sortAscending">Direção da ordenação (true para ascendente, false para descendente).</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Um ApiResponse contendo a lista paginada de entidades.</returns>
        Task<ApiResponse<PaginatedList<TEntity>>> GetPagedAsync(
            int pageIndex, 
            int pageSize, 
            string? searchTerm = null, 
            string? sortColumn = null, 
            bool sortAscending = true, 
            CancellationToken cancellationToken = default);
    }
} 