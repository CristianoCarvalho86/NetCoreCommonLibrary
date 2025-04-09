using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace NetCoreCommonLibrary.Util
{
    /// <summary>
    /// Extensões para facilitar comunicação com APIs externas
    /// </summary>
    public static class ApiExtensions
    {
        private static readonly JsonSerializerOptions DefaultJsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        /// <summary>
        /// Realiza uma requisição GET e converte a resposta para o tipo especificado
        /// </summary>
        public static async Task<T?> GetFromJsonAsync<T>(this HttpClient client, string endpoint, CancellationToken cancellationToken = default)
        {
            try
            {
                return await client.GetFromJsonAsync<T>(endpoint, DefaultJsonOptions, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                // Registre o erro em logs - você pode implementar um logger aqui
                Console.WriteLine($"Erro na requisição GET: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Realiza uma requisição POST com objeto JSON e converte a resposta para o tipo especificado
        /// </summary>
        public static async Task<TResponse?> PostAsJsonAsync<TRequest, TResponse>(
            this HttpClient client, 
            string endpoint, 
            TRequest content, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await client.PostAsJsonAsync(endpoint, content, DefaultJsonOptions, cancellationToken);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<TResponse>(DefaultJsonOptions, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erro na requisição POST: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Realiza uma requisição POST com objeto JSON
        /// </summary>
        public static async Task<HttpResponseMessage> PostAsJsonAsync<TRequest>(
            this HttpClient client, 
            string endpoint, 
            TRequest content, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await client.PostAsJsonAsync(endpoint, content, DefaultJsonOptions, cancellationToken);
                response.EnsureSuccessStatusCode();
                return response;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erro na requisição POST: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Realiza uma requisição PUT com objeto JSON
        /// </summary>
        public static async Task<HttpResponseMessage> PutAsJsonAsync<TRequest>(
            this HttpClient client, 
            string endpoint, 
            TRequest content, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await client.PutAsJsonAsync(endpoint, content, DefaultJsonOptions, cancellationToken);
                response.EnsureSuccessStatusCode();
                return response;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erro na requisição PUT: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Realiza uma requisição DELETE
        /// </summary>
        public static async Task<HttpResponseMessage> DeleteAsync(
            this HttpClient client, 
            string endpoint, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await client.DeleteAsync(endpoint, cancellationToken);
                response.EnsureSuccessStatusCode();
                return response;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erro na requisição DELETE: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Adiciona um token de autenticação do tipo Bearer ao HttpClient
        /// </summary>
        public static void AddBearerToken(this HttpClient client, string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Serializa um objeto para JSON
        /// </summary>
        public static string ToJson<T>(this T obj)
        {
            return JsonSerializer.Serialize(obj, DefaultJsonOptions);
        }

        /// <summary>
        /// Deserializa um JSON para o tipo especificado
        /// </summary>
        public static T? FromJson<T>(this string json)
        {
            return JsonSerializer.Deserialize<T>(json, DefaultJsonOptions);
        }
    }
} 