namespace NetCoreCommonLibrary.Models
{
    /// <summary>
    /// Representa uma lista de itens paginada.
    /// </summary>
    /// <typeparam name="T">O tipo dos itens na lista.</typeparam>
    public class PaginatedList<T>
    {
        /// <summary>
        /// Os itens da página atual.
        /// </summary>
        public IReadOnlyList<T> Items { get; }
        
        /// <summary>
        /// O índice da página atual (baseado em 1).
        /// </summary>
        public int PageIndex { get; }
        
        /// <summary>
        /// O número total de páginas disponíveis.
        /// </summary>
        public int TotalPages { get; }
        
        /// <summary>
        /// O número total de itens na fonte original.
        /// </summary>
        public int TotalCount { get; }

        // Construtor privado para forçar o uso do método de fábrica CreateAsync ou Create
        private PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            // Garante que TotalPages seja pelo menos 1 se houver itens
            if (TotalPages == 0 && count > 0) 
            {
                TotalPages = 1;
            }
            TotalCount = count;
            Items = items.AsReadOnly(); // Expõe como IReadOnlyList
        }

        /// <summary>
        /// Indica se existe uma página anterior.
        /// </summary>
        public bool HasPreviousPage => PageIndex > 1;
        
        /// <summary>
        /// Indica se existe uma próxima página.
        /// </summary>
        public bool HasNextPage => PageIndex < TotalPages;

        /// <summary>
        /// Cria uma instância de PaginatedList a partir de uma fonte de dados IEnumerable.
        /// ATENÇÃO: Este método carrega toda a fonte de dados na memória para contar os itens. 
        /// Prefira usar CreateAsync com IQueryable para fontes de dados de banco de dados.
        /// </summary>
        /// <param name="source">A fonte de dados IEnumerable.</param>
        /// <param name="pageIndex">O índice da página desejada (baseado em 1).</param>
        /// <param name="pageSize">O número de itens por página.</param>
        /// <returns>Uma instância de PaginatedList<T>.</returns>
        public static PaginatedList<T> Create(IEnumerable<T> source, int pageIndex, int pageSize)
        {
            ArgumentNullException.ThrowIfNull(source);
            if (pageIndex < 1) throw new ArgumentOutOfRangeException(nameof(pageIndex), "PageIndex deve ser maior ou igual a 1.");
            if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize), "PageSize deve ser maior ou igual a 1.");

            var count = source.Count(); // CUIDADO: Pode ser ineficiente para grandes coleções ou DB
            var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }

        /// <summary>
        /// Cria uma instância de PaginatedList de forma assíncrona a partir de uma fonte de dados IQueryable.
        /// Este método é mais eficiente para fontes de dados de banco de dados, pois a contagem e a paginação 
        /// são geralmente feitas no servidor.
        /// </summary>
        /// <param name="source">A fonte de dados IQueryable.</param>
        /// <param name="pageIndex">O índice da página desejada (baseado em 1).</param>
        /// <param name="pageSize">O número de itens por página.</param>
        /// <param name="cancellationToken">Token de cancelamento (opcional).</param>
        /// <returns>Uma Task contendo a instância de PaginatedList<T>.</returns>
        public static async Task<PaginatedList<T>> CreateAsync(
            IQueryable<T> source, 
            int pageIndex, 
            int pageSize, 
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            if (pageIndex < 1) throw new ArgumentOutOfRangeException(nameof(pageIndex), "PageIndex deve ser maior ou igual a 1.");
            if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize), "PageSize deve ser maior ou igual a 1.");

            // Executa a contagem no banco de dados de forma assíncrona
            var count = await source.CountAsync(cancellationToken);
            
            // Aplica a paginação no banco de dados e busca os itens de forma assíncrona
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
            
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
} 