using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NetCoreCommonLibrary.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreCommonLibrary.Services
{
    public class LanguageService : ILanguageService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<LanguageService> _logger;
        private const string CookieName = "preferred_language";
        private const string DefaultLanguage = "en"; // Inglês como padrão
        
        private static readonly Dictionary<string, CultureInfo> SupportedLanguages = new()
        {
            { "en", new CultureInfo("en-US") },    // Inglês (padrão)
            { "es", new CultureInfo("es-ES") },    // Espanhol
            { "de", new CultureInfo("de-DE") },    // Alemão
            { "it", new CultureInfo("it-IT") },    // Italiano
            { "hi", new CultureInfo("hi-IN") },    // Hindi
            { "pt", new CultureInfo("pt-PT") },    // Português (Portugal)
            { "br", new CultureInfo("pt-BR") },    // Português (Brasil)
            { "fr", new CultureInfo("fr-FR") }     // Francês
        };

        public LanguageService(IHttpContextAccessor httpContextAccessor, ILogger<LanguageService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public string GetCurrentLanguageCode()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            
            if (httpContext == null)
            {
                _logger.LogWarning("HttpContext is null, returning default language code");
                return DefaultLanguage;
            }
            
            // 1. Verifique o cookie
            if (httpContext.Request.Cookies.TryGetValue(CookieName, out var cookieLanguage) && 
                IsLanguageSupported(cookieLanguage))
            {
                return cookieLanguage.ToLowerInvariant();
            }
            
            // 2. Verifique o parâmetro de consulta
            if (httpContext.Request.Query.TryGetValue("lang", out var queryLanguage) &&
                IsLanguageSupported(queryLanguage.ToString()))
            {
                return queryLanguage.ToString().ToLowerInvariant();
            }
            
            // 3. Verifique o cabeçalho Accept-Language
            var acceptLanguages = httpContext.Request.Headers["Accept-Language"].ToString();
            if (!string.IsNullOrEmpty(acceptLanguages))
            {
                var languages = acceptLanguages.Split(',')
                    .Select(l => l.Split(';')[0].Trim())
                    .Select(l => l.Split('-')[0])
                    .ToList();
                
                foreach (var language in languages)
                {
                    if (IsLanguageSupported(language))
                    {
                        return language.ToLowerInvariant();
                    }
                }
            }
            
            // 4. Retorne o idioma padrão
            return DefaultLanguage;
        }

        public CultureInfo GetCurrentCulture()
        {
            var languageCode = GetCurrentLanguageCode();
            return SupportedLanguages.TryGetValue(languageCode, out var culture) 
                ? culture 
                : SupportedLanguages[DefaultLanguage];
        }

        public async Task SetLanguageAsync(string languageCode)
        {
            if (!IsLanguageSupported(languageCode))
            {
                _logger.LogWarning("Unsupported language code: {LanguageCode}", languageCode);
                return;
            }
            
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    HttpOnly = true,
                    Secure = httpContext.Request.IsHttps,
                    SameSite = SameSiteMode.Lax
                };
                
                httpContext.Response.Cookies.Append(CookieName, languageCode.ToLowerInvariant(), cookieOptions);
                
                // Defina a cultura atual para esta solicitação
                var cultureInfo = SupportedLanguages[languageCode.ToLowerInvariant()];
                CultureInfo.CurrentCulture = cultureInfo;
                CultureInfo.CurrentUICulture = cultureInfo;
            }
            
            await Task.CompletedTask;
        }

        public bool IsLanguageSupported(string languageCode)
        {
            return !string.IsNullOrEmpty(languageCode) && 
                   SupportedLanguages.ContainsKey(languageCode.ToLowerInvariant());
        }
    }
} 