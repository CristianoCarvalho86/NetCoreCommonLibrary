using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace NetCoreCommonLibrary.Util
{
    /// <summary>
    /// Helper para trabalhar com tokens JWT
    /// </summary>
    public static class JwtHelper
    {
        /// <summary>
        /// Decodifica um token JWT e retorna as claims
        /// </summary>
        /// <param name="token">O token JWT.</param>
        /// <returns>Uma coleção de claims ou null se o token for inválido.</returns>
        public static IEnumerable<Claim>? DecodeToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var handler = new JwtSecurityTokenHandler();
            
            // Verifica se o handler pode ler o token antes de tentar
            if (!handler.CanReadToken(token))
                return null;

            try
            {
                var jwtToken = handler.ReadJwtToken(token);
                return jwtToken.Claims;
            }
            catch (ArgumentException ex) // Captura exceções específicas de token malformado
            {
                Console.WriteLine($"Erro ao decodificar token JWT: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Obtém o valor de uma claim específica do token
        /// </summary>
        /// <param name="token">O token JWT.</param>
        /// <param name="claimType">O tipo da claim desejada.</param>
        /// <returns>O valor da claim ou null se não encontrada.</returns>
        public static string? GetClaimValue(string token, string claimType)
        {
            var claims = DecodeToken(token);
            return claims?.FirstOrDefault(c => c.Type == claimType)?.Value;
        }

        /// <summary>
        /// Verifica se o token está expirado (baseado na claim 'exp')
        /// </summary>
        /// <param name="token">O token JWT.</param>
        /// <returns>True se o token está expirado ou a claim 'exp' é inválida, False caso contrário.</returns>
        public static bool IsTokenExpired(string token)
        {
            var expirationClaim = GetClaimValue(token, JwtRegisteredClaimNames.Exp);
            
            if (string.IsNullOrEmpty(expirationClaim))
                return true; // Considera expirado se não houver claim 'exp'

            if (long.TryParse(expirationClaim, out long expirationTimeUnix))
            {
                // Converte o tempo Unix (segundos) para DateTime UTC
                var expirationDateTimeUtc = DateTimeOffset.FromUnixTimeSeconds(expirationTimeUnix).UtcDateTime;
                return expirationDateTimeUtc <= DateTime.UtcNow;
            }

            return true; // Considera expirado se a claim 'exp' não for um número long válido
        }

        /// <summary>
        /// Obtém o ID do usuário a partir do token (procura por NameIdentifier e sub)
        /// </summary>
        /// <param name="token">O token JWT.</param>
        /// <returns>O ID do usuário ou null se não encontrado.</returns>
        public static string? GetUserId(string token)
        {
            return GetClaimValue(token, ClaimTypes.NameIdentifier) ?? GetClaimValue(token, JwtRegisteredClaimNames.Sub);
        }

        /// <summary>
        /// Obtém o nome do usuário a partir do token (procura por Name e name)
        /// </summary>
        /// <param name="token">O token JWT.</param>
        /// <returns>O nome do usuário ou null se não encontrado.</returns>
        public static string? GetUserName(string token)
        {
            return GetClaimValue(token, ClaimTypes.Name) ?? GetClaimValue(token, "name");
        }

        /// <summary>
        /// Obtém os papéis/funções (roles) do usuário a partir do token
        /// </summary>
        /// <param name="token">O token JWT.</param>
        /// <returns>Uma coleção de roles ou vazia se não houver.</returns>
        public static IEnumerable<string> GetUserRoles(string token)
        {
            var claims = DecodeToken(token);
            
            if (claims == null)
                return Enumerable.Empty<string>();

            return claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role") // Considera ambos os tipos comuns de claim de role
                .Select(c => c.Value)
                .Where(r => !string.IsNullOrEmpty(r)) // Filtra roles vazias ou nulas
                .ToList();
        }

        /// <summary>
        /// Verifica se o usuário possui um determinado papel/função (role)
        /// </summary>
        /// <param name="token">O token JWT.</param>
        /// <param name="role">O nome do role a ser verificado (case-insensitive).</param>
        /// <returns>True se o usuário possui o role, False caso contrário.</returns>
        public static bool UserHasRole(string token, string role)
        {
            if (string.IsNullOrEmpty(role))
                return false;
            return GetUserRoles(token).Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Valida um token JWT usando uma chave secreta e, opcionalmente, issuer e audience.
        /// </summary>
        /// <param name="token">O token JWT a ser validado.</param>
        /// <param name="secretKey">A chave secreta usada para assinar o token.</param>
        /// <param name="issuer">O issuer esperado (opcional).</param>
        /// <param name="audience">A audience esperada (opcional).</param>
        /// <returns>True se o token for válido, False caso contrário.</returns>
        public static bool ValidateToken(string token, string secretKey, string? issuer = null, string? audience = null)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(secretKey))
                return false;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);
            
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = !string.IsNullOrEmpty(issuer),
                ValidIssuer = issuer,
                ValidateAudience = !string.IsNullOrEmpty(audience),
                ValidAudience = audience,
                ValidateLifetime = true, // Garante que o token não expirou
                ClockSkew = TimeSpan.Zero // Não permite tolerância de tempo
            };

            try
            {
                // Tenta validar o token
                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return validatedToken != null;
            }
            catch (SecurityTokenException ex) // Captura exceções específicas de validação
            {
                Console.WriteLine($"Falha na validação do token: {ex.Message}");
                return false;
            }
            catch (Exception ex) // Captura outras exceções inesperadas
            {
                Console.WriteLine($"Erro inesperado durante a validação do token: {ex.Message}");
                return false;
            }
        }
    }
} 