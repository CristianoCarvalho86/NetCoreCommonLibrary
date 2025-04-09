using System.Linq.Expressions;

namespace NetCoreCommonLibrary.Util
{
    /// <summary>
    /// Extensões para trabalhar com coleções
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Converte uma coleção em uma lista paginada
        /// </summary>
        public static NetCoreCommonLibrary.Models.PaginatedList<T> ToPaginatedList<T>(
            this IEnumerable<T> source, 
            int pageIndex, 
            int pageSize)
        {
            return NetCoreCommonLibrary.Models.PaginatedList<T>.Create(source, pageIndex, pageSize);
        }

        /// <summary>
        /// Ordena uma coleção por uma propriedade específica
        /// </summary>
        public static IOrderedEnumerable<T> OrderByProperty<T>(
            this IEnumerable<T> source, 
            string propertyName, 
            bool ascending = true)
        {
            // Validação de entradas
            if (source == null) 
                throw new ArgumentNullException(nameof(source));
            
            if (string.IsNullOrEmpty(propertyName)) 
                throw new ArgumentException("O nome da propriedade não pode ser nulo ou vazio", nameof(propertyName));

            // Obtém o tipo de T
            var type = typeof(T);
            
            // Obtém a propriedade pelo nome
            var property = type.GetProperty(propertyName);
            
            if (property == null)
                throw new ArgumentException($"Propriedade '{propertyName}' não encontrada no tipo {type.Name}");

            // Cria uma expressão lambda para acessar a propriedade
            var parameter = Expression.Parameter(type, "x");
            var propertyAccess = Expression.Property(parameter, property);
            var lambda = Expression.Lambda(propertyAccess, parameter);

            // Método genérico OrderBy/OrderByDescending
            var orderByMethod = ascending ?
                typeof(Enumerable).GetMethods()
                    .First(m => m.Name == "OrderBy" && m.GetParameters().Length == 2) :
                typeof(Enumerable).GetMethods()
                    .First(m => m.Name == "OrderByDescending" && m.GetParameters().Length == 2);

            // Tornando genérico para o tipo da propriedade
            var orderByGeneric = orderByMethod.MakeGenericMethod(type, property.PropertyType);

            // Invoca o método OrderBy/OrderByDescending
            return (IOrderedEnumerable<T>)orderByGeneric.Invoke(null, new object[] { source, lambda.Compile() })!;
        }

        /// <summary>
        /// Verifica se uma coleção está vazia
        /// </summary>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
        {
            return source == null || !source.Any();
        }

        /// <summary>
        /// Executa uma ação para cada item da coleção
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (action == null) throw new ArgumentNullException(nameof(action));

            foreach (var item in source)
            {
                action(item);
            }
        }

        /// <summary>
        /// Executa uma ação assíncrona para cada item da coleção
        /// </summary>
        public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> action)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (action == null) throw new ArgumentNullException(nameof(action));

            foreach (var item in source)
            {
                await action(item);
            }
        }

        /// <summary>
        /// Retorna uma nova coleção com itens distintos baseados em uma propriedade
        /// </summary>
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(keySelector);

            return DistinctByIterator(source, keySelector);
        }

        private static IEnumerable<T> DistinctByIterator<T, TKey>(
            IEnumerable<T> source, 
            Func<T, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (var element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
} 