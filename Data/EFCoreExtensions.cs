using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreCommonLibrary.Data
{
    /// <summary>
    /// Extensões para Entity Framework Core que facilitam operações comuns.
    /// </summary>
    public static class EFCoreExtensions
    {
        /// <summary>
        /// Obtém uma entidade por ID com inclusões opcionais.
        /// </summary>
        /// <typeparam name="T">O tipo da entidade.</typeparam>
        /// <typeparam name="TKey">O tipo da chave primária.</typeparam>
        /// <param name="dbSet">O DbSet para consultar.</param>
        /// <param name="id">O ID da entidade.</param>
        /// <param name="includes">Expressões de inclusão opcionais.</param>
        /// <returns>A entidade encontrada ou null.</returns>
        public static async Task<T?> GetByIdAsync<T, TKey>(
            this DbSet<T> dbSet,
            TKey id,
            params Expression<Func<T, object>>[] includes) where T : class
        {
            IQueryable<T> query = dbSet;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            // Assumindo que a entidade tem uma propriedade Id ou ID
            // Esta é uma simplificação - em um cenário real, você precisaria 
            // ter conhecimento sobre a entidade específica
            return await query.FirstOrDefaultAsync(e => EF.Property<TKey>(e, "Id").Equals(id));
        }

        /// <summary>
        /// Obtém uma página de resultados com ordenação.
        /// </summary>
        /// <typeparam name="T">O tipo da entidade.</typeparam>
        /// <param name="query">A consulta IQueryable base.</param>
        /// <param name="pageNumber">O número da página (começando em 1).</param>
        /// <param name="pageSize">O tamanho da página.</param>
        /// <param name="orderBy">A expressão de ordenação.</param>
        /// <param name="orderByDescending">Se verdadeiro, ordena em ordem decrescente.</param>
        /// <param name="cancellationToken">Token de cancelamento opcional.</param>
        /// <returns>Uma página de resultados.</returns>
        public static async Task<PagedResult<T>> ToPagedListAsync<T>(
            this IQueryable<T> query,
            int pageNumber,
            int pageSize,
            Expression<Func<T, object>> orderBy,
            bool orderByDescending = false,
            CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var totalCount = await query.CountAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            if (orderBy != null)
            {
                query = orderByDescending 
                    ? query.OrderByDescending(orderBy) 
                    : query.OrderBy(orderBy);
            }

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<T>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        /// <summary>
        /// Aplica critérios de filtro dinâmicos a uma consulta.
        /// </summary>
        /// <typeparam name="T">O tipo da entidade.</typeparam>
        /// <param name="query">A consulta IQueryable base.</param>
        /// <param name="filters">Expressões de filtro a serem aplicadas.</param>
        /// <returns>A consulta filtrada.</returns>
        public static IQueryable<T> ApplyFilters<T>(
            this IQueryable<T> query,
            params Expression<Func<T, bool>>[] filters)
        {
            if (filters == null || filters.Length == 0)
            {
                return query;
            }

            foreach (var filter in filters)
            {
                if (filter != null)
                {
                    query = query.Where(filter);
                }
            }

            return query;
        }

        /// <summary>
        /// Executa upsert (inserir ou atualizar) de uma entidade baseado em uma condição.
        /// </summary>
        /// <typeparam name="T">O tipo da entidade.</typeparam>
        /// <param name="dbSet">O DbSet para operar.</param>
        /// <param name="entity">A entidade a ser inserida ou atualizada.</param>
        /// <param name="matchPredicate">O predicado para verificar se a entidade já existe.</param>
        /// <param name="updateAction">Ação opcional para personalizar a atualização.</param>
        /// <returns>A entidade após o upsert.</returns>
        public static async Task<T> UpsertAsync<T>(
            this DbSet<T> dbSet,
            T entity,
            Expression<Func<T, bool>> matchPredicate,
            Action<T, T>? updateAction = null) where T : class
        {
            var existingEntity = await dbSet.FirstOrDefaultAsync(matchPredicate);

            if (existingEntity == null)
            {
                // Inserir
                await dbSet.AddAsync(entity);
                return entity;
            }
            else
            {
                // Atualizar
                if (updateAction != null)
                {
                    // Executa a ação de atualização personalizada
                    updateAction(existingEntity, entity);
                }
                else
                {
                    // Copia todas as propriedades, exceto a chave primária e propriedades de navegação
                    var entry = dbSet.Entry(existingEntity);
                    entry.CurrentValues.SetValues(entity);
                }

                return existingEntity;
            }
        }

        /// <summary>
        /// Adiciona uma coleção de entidades em lote.
        /// </summary>
        /// <typeparam name="T">O tipo da entidade.</typeparam>
        /// <param name="dbSet">O DbSet para operar.</param>
        /// <param name="entities">As entidades a serem adicionadas.</param>
        /// <param name="batchSize">O tamanho do lote (default: 100).</param>
        /// <param name="cancellationToken">Token de cancelamento opcional.</param>
        /// <returns>Um task que representa a operação assíncrona.</returns>
        public static async Task AddRangeInBatchesAsync<T>(
            this DbSet<T> dbSet,
            IEnumerable<T> entities,
            int batchSize = 100,
            CancellationToken cancellationToken = default) where T : class
        {
            var items = entities.ToList();
            for (int i = 0; i < items.Count; i += batchSize)
            {
                var batch = items.Skip(i).Take(batchSize).ToList();
                await dbSet.AddRangeAsync(batch, cancellationToken);
            }
        }

        /// <summary>
        /// Adiciona funções de soft delete (exclusão lógica) a um DbContext.
        /// Assume que as entidades implementam ISoftDelete.
        /// </summary>
        /// <param name="modelBuilder">O modelBuilder para configurar.</param>
        public static void ApplySoftDeleteQueryFilter(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Verifica se o tipo implementa ISoftDelete
                if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var property = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
                    var falseConstant = Expression.Constant(false);
                    var comparison = Expression.Equal(property, falseConstant);
                    var lambda = Expression.Lambda(comparison, parameter);

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }

        /// <summary>
        /// Salva alterações de forma resiliente, com tentativas em caso de falha.
        /// </summary>
        /// <param name="context">O DbContext.</param>
        /// <param name="maxRetryCount">Número máximo de tentativas.</param>
        /// <param name="cancellationToken">Token de cancelamento opcional.</param>
        /// <returns>O número de entidades afetadas.</returns>
        public static async Task<int> SaveChangesResillientAsync(
            this DbContext context,
            int maxRetryCount = 3,
            CancellationToken cancellationToken = default)
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    return await context.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateConcurrencyException ex) when (retryCount < maxRetryCount)
                {
                    retryCount++;
                    foreach (var entry in ex.Entries)
                    {
                        // Atualiza os valores originais para os valores atuais do banco
                        await entry.ReloadAsync(cancellationToken);
                    }
                }
                catch (Exception) when (retryCount < maxRetryCount)
                {
                    retryCount++;
                    await Task.Delay(100 * retryCount, cancellationToken);
                }
            }
        }
    }

    /// <summary>
    /// Resultado paginado para consultas.
    /// </summary>
    /// <typeparam name="T">O tipo do item na lista paginada.</typeparam>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    /// <summary>
    /// Interface para entidades que suportam soft delete.
    /// </summary>
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedAt { get; set; }
        string? DeletedBy { get; set; }
    }
} 