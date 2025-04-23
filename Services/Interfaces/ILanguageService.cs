using System.Globalization;
using System.Threading.Tasks;

namespace NetCoreCommonLibrary.Services.Interfaces
{
    public interface ILanguageService
    {
        /// <summary>
        /// Obtém o código do idioma atual (ex: "en", "es", "de")
        /// </summary>
        string GetCurrentLanguageCode();
        
        /// <summary>
        /// Obtém a cultura do idioma atual
        /// </summary>
        CultureInfo GetCurrentCulture();
        
        /// <summary>
        /// Define o idioma atual usando um código de idioma (ex: "en", "es", "de")
        /// </summary>
        Task SetLanguageAsync(string languageCode);
        
        /// <summary>
        /// Verifica se o idioma é suportado
        /// </summary>
        bool IsLanguageSupported(string languageCode);
    }
} 