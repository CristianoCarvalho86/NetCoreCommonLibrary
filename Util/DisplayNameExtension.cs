using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace NetCoreCommonLibrary.Util
{
    public static class DisplayNameExtension
    {
        /// <summary>
        /// Obtém o nome de exibição de uma propriedade de um tipo específico.
        /// </summary>
        /// <param name="propertyName">O nome da propriedade.</param>
        /// <param name="type">O tipo que contém a propriedade.</param>
        /// <returns>O nome de exibição ou o nome da propriedade se não encontrado.</returns>
        public static string GetDisplayName(this string propertyName, Type type)
        {
            if (string.IsNullOrEmpty(propertyName) || type == null)
                return propertyName ?? string.Empty; // Retorna string vazia se propertyName for nulo

            // Buscar a propriedade no tipo fornecido
            var property = type.GetProperty(propertyName);
            
            if (property == null)
                return propertyName;

            // Buscar o atributo Display
            var attribute = property.GetCustomAttribute<DisplayAttribute>();
            
            // Retorna o nome do atributo ou o nome da propriedade se o atributo ou seu nome for nulo
            return attribute?.Name ?? propertyName;
        }

        /// <summary>
        /// Obtém o nome de exibição de uma propriedade usando um tipo genérico.
        /// </summary>
        /// <typeparam name="T">O tipo que contém a propriedade.</typeparam>
        /// <param name="propertyName">O nome da propriedade.</param>
        /// <returns>O nome de exibição ou o nome da propriedade se não encontrado.</returns>
        public static string GetDisplayName<T>(this string propertyName)
        {
            return GetDisplayName(propertyName, typeof(T));
        }
    }
} 